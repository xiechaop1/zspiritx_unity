using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FieldStageManager))]
public class FieldEntityManager : MonoBehaviour, IManager {
	public GameObject ARSceneScanHint;
	public GameObject goCamera;//必须要在mainCam下创建一个空go，mainCam的坐标会在update被AR Foundation上锁
	public GameObject goCamDir;
	public GameObject prefabARDir;
	public GameObject goRoot;
	public GameObject goStorage;
	private ARPlaneInfo arPlane;
	Queue<FieldEntityInfo> queFieldEntity = new Queue<FieldEntityInfo>();
	List<FieldEntityInfo> queTaggedEntity = new List<FieldEntityInfo>();
	Queue<FieldEntityInfo> queLatLonEntity = new Queue<FieldEntityInfo>();
	List<FieldEntityInfo> lstPlacedEntity = new List<FieldEntityInfo>();
	public FieldEntityInfo[] arrPlacedEntity => lstPlacedEntity.ToArray();
	public bool isLoadFinish => queFieldEntity.Count == 0;

	public float maxShowDistance = 1.5f;

	private EntityActionManager actionManager;
	private FieldStageManager stageManager;
	private SceneLoadManager loadManager;
	private InputGPSManager geolocManager;

	private bool isPlaneVisible = false;
	private float timeLastCheck = 1f;

	private void Update() {
		goCamDir.transform.position = goCamera.transform.position;// + Vector3.down;
		Vector3 fwd = goCamera.transform.forward + goCamera.transform.up;
		fwd.y = 0;
		goCamDir.transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
		if (timeLastCheck < 0) {
			if (!loadManager.isLoading && loadManager.sceneMode == SceneLoadManager.SceneMode.AR) {
				if (queFieldEntity.Count > 0) {
					TryPlaceRamdomEntities(10);
				}
			}
			timeLastCheck = 2f;
		} else if (timeLastCheck > 1f && timeLastCheck < 1.5f) {
			if (!loadManager.isLoading && loadManager.sceneMode == SceneLoadManager.SceneMode.AR) {
				if (queLatLonEntity.Count > 0) {
					PlaceGeoLocEntities(10);
				}
			}
			timeLastCheck = 0.5f;
		} else {
			timeLastCheck -= Time.deltaTime;
		}
	}

	public void Init(UIEventManager eventManager, params IManager[] managers) {
		eventManager.RegisteredAction("FieldEntityManager", "RegisterARPlane", RegisterARPlane);
		eventManager.RegisteredAction("FieldEntityManager", "RemoveARPlane", RemoveARPlane);
		eventManager.RegisteredAction("FieldEntityManager", "RemoveFieldEntitys", RemoveFieldEntitys);
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
	}
	public void RegisterManager(IManager manager) {
		if (manager is EntityActionManager) {
			actionManager = manager as EntityActionManager;
		} else if (manager is FieldStageManager) {
			stageManager = manager as FieldStageManager;
		} else if (manager is SceneLoadManager) {
			loadManager = manager as SceneLoadManager;
		} else if (manager is InputGPSManager) {
			geolocManager = manager as InputGPSManager;
		}
	}

	#region Staging Activities
	public void PrepareScene() {
		PrepareStage();
		isPlaneVisible = true;
		foreach (ARPlaneInfo plane in planeHorizontal) {
			if (plane != null) {
				plane.SetPlaneVisibility(isPlaneVisible);
			}
		}
		foreach (ARPlaneInfo plane in planeVertical) {
			if (plane != null) {
				plane.SetPlaneVisibility(isPlaneVisible);
			}
		}
	}
	public void PrepareStage(GameObject stageRoot = null) {
		foreach (FieldEntityInfo entityInfo in stageManager.lstFieldEntity) {
			if (PrepareEntity(entityInfo)) {
				queFieldEntity.Enqueue(entityInfo);
			}
		}
		foreach (FieldEntityInfo entityInfo in stageManager.lstTaggedEntity) {
			if (PrepareEntity(entityInfo)) {
				queTaggedEntity.Add(entityInfo);
			}
		}
		foreach (FieldEntityInfo entityInfo in stageManager.lstLocEntity) {
			if (PrepareEntity(entityInfo)) {
				queLatLonEntity.Enqueue(entityInfo);
			}
		}
	}

	private bool PrepareEntity(FieldEntityInfo info) {
		if (info == null) {
			return false;
		}
		info.entityManager = this;
		if (info.TryGetComponent(out ARInteractListener listener)) {
			listener.SetActionManager(actionManager);
		}
		return true;
	}

	public void StopScene() {
		StopStage();

		isPlaneVisible = false;

		foreach (ARPlaneInfo plane in planeHorizontal) {
			if (plane != null) {
				plane.SetPlaneVisibility(isPlaneVisible);
			}
		}
		foreach (ARPlaneInfo plane in planeVertical) {
			if (plane != null) {
				plane.SetPlaneVisibility(isPlaneVisible);
			}
		}
		ARSceneScanHint.SetActive(false);
		stageManager.CleanBackstage();
	}
	public void StopStage() {
		FieldEntityInfo entityInfo;
		while (queFieldEntity.Count > 0) {
			entityInfo = queFieldEntity.Dequeue();
			if (entityInfo != null) {
				StopEntity(entityInfo);
			}
		}
		for (int i = queTaggedEntity.Count - 1; i >= 0; i--) {
			entityInfo = queTaggedEntity[i];
			queTaggedEntity.RemoveAt(i);
			if (entityInfo != null) {
				StopEntity(entityInfo);
			}
		}
		while (queLatLonEntity.Count > 0) {
			entityInfo = queLatLonEntity.Dequeue();
			if (entityInfo != null) {
				StopEntity(entityInfo);
			}
		}
		for (int i = lstPlacedEntity.Count - 1; i >= 0; i--) {
			entityInfo = lstPlacedEntity[i];
			lstPlacedEntity.RemoveAt(i);
			if (entityInfo != null) {
				entityInfo.RemoveFromField();
				StopEntity(entityInfo);
			}
		}
	}

	private void StopEntity(FieldEntityInfo entityInfo) {
		if (entityInfo == null) {
			return;
		}
		OnEntityRemoved.Invoke(entityInfo);
	}
	#endregion

	#region ARPlane
	private List<ARPlaneInfo> planeHorizontal = new List<ARPlaneInfo>();
	private List<ARPlaneInfo> planeVertical = new List<ARPlaneInfo>();
	private float areaFloor = 0f;
	private float areaWall = 0f;

	public void ARPlaneUpdated(ARPlaneInfo arPlane) {
		if (arPlane.isHorizontal) {
			OnFloorUpdate();
		} else {
			OnWallUpdate();
		}
	}
	private float CalculateArea(List<ARPlaneInfo> planes) {
		float sum = 0f;
		foreach (var plane in planes) {
			sum += plane.area;
		}
		return sum;
	}
	public void RegisterARPlane(IEventMessage arInfo) {
		ARPlaneInfo arPlane = arInfo as ARPlaneInfo;
		if (arPlane.isHorizontal) {
			if (!planeHorizontal.Contains(arPlane)) {
				planeHorizontal.Add(arPlane);
			}
		} else {
			if (!planeVertical.Contains(arPlane)) {
				planeVertical.Add(arPlane);
			}
		}
		arPlane.SetPlaneVisibility(isPlaneVisible);
		arPlane.planeUpdated += ARPlaneUpdated;
		ARPlaneUpdated(arPlane);
	}

	public void RemoveARPlane(IEventMessage arInfo) {
		ARPlaneInfo arPlane = arInfo as ARPlaneInfo;
		if (arPlane.isHorizontal) {
			if (!planeHorizontal.Contains(arPlane)) {
				planeHorizontal.Remove(arPlane);
			}
		} else {
			if (!planeVertical.Contains(arPlane)) {
				planeVertical.Remove(arPlane);
			}
		}
		ARPlaneUpdated(arPlane);
	}
	private void OnFloorUpdate() {
		areaFloor = CalculateArea(planeHorizontal);
		foreach (var entity in entityHorizontal) {
			if (entity) {
				ShiftOnNormal(entity);
			}
		}
	}
	private void OnWallUpdate() {
		areaWall = CalculateArea(planeVertical);
		foreach (var entity in entityVertical) {
			if (entity) {
				ShiftOnNormal(entity);
			}
		}
	}

	#endregion

	#region GeoLoc Pre-process
	//List<GameObject> geolocDir = new List<GameObject>();
	public void PlaceGeoLocEntities(int maxTries) {
		GameObject obj;
		FieldEntityInfo entityInfo;
		for (int i = 0; i < maxTries; i++) {
			if (queLatLonEntity.Count <= 0) {
				break;
			}

			entityInfo = queLatLonEntity.Dequeue();
			obj = geolocManager.GetPosObject(entityInfo.latitude, entityInfo.longitude, maxShowDistance);
			if (obj == null) {
				queLatLonEntity.Enqueue(entityInfo);
			} else if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.GeoLocPosition) {
				entityInfo.goReference = obj;
				ForcePlacing(entityInfo);
			} else if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.GeoLocAround) {
				entityInfo.goReference = obj;
				queFieldEntity.Enqueue(entityInfo);
			}

		}
	}
	public void PlaceGeoLocEntity() {
	}
	#endregion

	#region ARTag Pre-process
	Dictionary<string, GameObject> imageDirs = new Dictionary<string, GameObject>();
	Dictionary<string, GameObject> imageTrackers = new Dictionary<string, GameObject>();
	public void PlaceImageTrackingEntity(string imgName, GameObject goImageTracker) {
		GameObject obj;
		if (imageDirs.ContainsKey(imgName)) {

		} else {
			obj = Instantiate(prefabARDir);
			obj.name = imgName + "-ImagDir";
			obj.GetComponent<ImageDirMover>().Init(goImageTracker);
			obj.transform.parent = transform;
			imageDirs.Add(imgName, obj);
		}
		if (imageTrackers.ContainsKey(imgName)) {
			if (goImageTracker != null) {
				imageTrackers[imgName] = goImageTracker;
			}
		} else {
			imageTrackers.Add(imgName, goImageTracker);
		}
		UpdateImageTracking(imgName);
	}

	public void UpdateImageTracking(string imgName) {
		if (queTaggedEntity.Count == 0) {
			return;
		}
		for (int i = queTaggedEntity.Count - 1; i >= 0; i--) {
			FieldEntityInfo entityInfo = queTaggedEntity[i];
			if (entityInfo.uuidImageTracking == imgName) {
				if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.ARTagTracking) {
					entityInfo.transform.parent = imageTrackers[imgName].transform;
					entityInfo.transform.localPosition = Vector3.zero;
					entityInfo.transform.localRotation = Quaternion.identity;
					queTaggedEntity.RemoveAt(i);
					lstPlacedEntity.Add(entityInfo);
					if (OnEntityPlaced != null) {
						OnEntityPlaced.Invoke(entityInfo);
					}
				} else if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.ARTagAround) {
					entityInfo.goReference = imageDirs[imgName];
					queTaggedEntity.RemoveAt(i);
					queFieldEntity.Enqueue(entityInfo);
				} else if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.ARTagPosition) {
					entityInfo.goReference = imageDirs[imgName];
					queTaggedEntity.RemoveAt(i);
					ForcePlacing(entityInfo);
				}
			}
		}
	}

	public GameObject GetStageDir() {
		if (stageManager.currentStage.stageToggleType == FieldStageInfo.StageToggleType.ARTag && imageDirs.ContainsKey(stageManager.currentStage.uuidARTag)) {
			return imageDirs[stageManager.currentStage.uuidARTag];
		}
		return null;
	}
	#endregion

	#region Place-At
	private void ForcePlacing(FieldEntityInfo entityInfo) {
		entityInfo.ForcePlacing(goRoot.transform);
		lstPlacedEntity.Add(entityInfo);
		if (OnEntityPlaced != null) {
			OnEntityPlaced.Invoke(entityInfo);
		}
	}
	#endregion

	#region Place-Around
	private List<FieldEntityInfo> entityHorizontal = new List<FieldEntityInfo>();
	private List<FieldEntityInfo> entityVertical = new List<FieldEntityInfo>();

	public void ShiftOnNormal(FieldEntityInfo entity) {
		Ray ray = new Ray(entity.transform.position + entity.transform.up * 0.5f, -entity.transform.up);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 1f, 8)) {
			if ((hit.point - entity.transform.position).sqrMagnitude > 0.0001f) {
				if (entity.TryPlacing(hit.point, entity.transform.rotation, hit.collider.gameObject, goRoot.transform)) {

				}
			}
		}

	}

	public Action<FieldEntityInfo> OnEntityPlaced;
	public Action<FieldEntityInfo> OnEntityRemoved;

	public void RemoveFieldEntitys(IEventMessage arInfo) {
		FieldEntityInfo entityInfo = arInfo as FieldEntityInfo;
		if (entityInfo == null) {
			return;
		}
		if (entityHorizontal.Contains(entityInfo)) {
			entityHorizontal.Remove(entityInfo);
		} else if (entityVertical.Contains(entityInfo)) {
			entityVertical.Remove(entityInfo);
		}
		if (lstPlacedEntity.Contains(entityInfo)) {
			lstPlacedEntity.Remove(entityInfo);
		}
		stageManager.OnRemoveFieldEntity(entityInfo);
		if (OnEntityRemoved != null) {
			OnEntityRemoved.Invoke(entityInfo);
		}
	}
	public void TryPlaceRamdomEntities(int maxTries) {
		FieldEntityInfo entityInfo;
		for (int i = 0; i < maxTries; i++) {
			if (queFieldEntity.Count <= 0) {
				break;
			}

			entityInfo = queFieldEntity.Dequeue();
			if (!entityInfo.goReference) {
				entityInfo.goReference = goCamDir;
			}
			if (TryPlaceEntityAround(entityInfo, 1f)) {
				if (OnEntityPlaced != null) {
					OnEntityPlaced.Invoke(entityInfo);
				}
				lstPlacedEntity.Add(entityInfo);
			} else {
				queFieldEntity.Enqueue(entityInfo);
			}
		}

		if (queFieldEntity.Count <= 0) {
			ARSceneScanHint.SetActive(false);
		} else if (!ARSceneScanHint.activeInHierarchy) {
			ARSceneScanHint.SetActive(true);
		}
	}
	public bool TryPlaceEntityAround(FieldEntityInfo entityInfo, float marginError) {
		(bool hasPos, RaycastHit hit, int enumSurfaceType) = TryGetPos(entityInfo.IdealPos, entityInfo.enumSurfaceType, marginError);
		if (hasPos) {
			if (enumSurfaceType == 1) {
				if (entityInfo.TryPlacing(hit.point, entityInfo.goReference.transform.rotation, arPlane.gameObject, goRoot.transform)) {
					entityHorizontal.Add(entityInfo);
					return true;
				}
			} else if (enumSurfaceType == 2) {
				if (entityInfo.TryPlacing(hit.point, hit.collider.gameObject.transform.rotation, arPlane.gameObject, goRoot.transform)) {
					entityVertical.Add(entityInfo);
					return true;
				}
			}
		}
		return false;
	}

	(bool, RaycastHit, int) TryGetPos(Vector3 origin, int enumSurfaceType, float marginError) {
		RaycastHit hit = new RaycastHit();
		//bool isHorizontal;
		if (enumSurfaceType == 1) {
			for (int i = 0; i < 50; i++) {
				//Debug.Log("tryGetPos horizontal, attempt: " + i);
				if (Physics.Raycast(origin + new Vector3(UnityEngine.Random.Range(-marginError, marginError), 1f, UnityEngine.Random.Range(-marginError, marginError)), Vector3.down, out hit, 4f, 8)) {
					if (hit.collider.gameObject.TryGetComponent(out arPlane) && arPlane.isHorizontal) {
						return (true, hit, 1);
					}
				}
			}
		} else if (enumSurfaceType == 2) {
			Ray ray;
			for (int i = 0; i < 50; i++) {
				//Debug.Log("tryGetPos omni, attempt: " + i);
				if (Physics.Raycast(origin, new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f)).normalized, out hit, marginError, 8)) {
					if (hit.collider.gameObject.TryGetComponent(out arPlane) && !arPlane.isHorizontal) {
						ray = new Ray(hit.point + arPlane.up * 0.01f + new Vector3(0, UnityEngine.Random.Range(-0.5f, 0.5f), 0), -arPlane.up);
						//Debug.Log("wallfound at: "+ hit.point+" raycast from: "+ray.origin);
						if (Physics.Raycast(ray, out hit, 0.1f) && hit.collider.gameObject.TryGetComponent(out arPlane) && !arPlane.isHorizontal) {
							return (true, hit, 2);
						}
					}
				}
			}
		} else if (enumSurfaceType == -1) {
			for (int i = 0; i < 50; i++) {
				//Debug.Log("tryGetPos omni, attempt: " + i);
				if (Physics.Raycast(origin, new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f)).normalized, out hit, marginError, 8)) {
					if (hit.collider.gameObject.TryGetComponent(out arPlane)) {
						//isHorizontal = Physics.Raycast(hit.point + new Vector3(0, 0.01f, 0), Vector3.down);
						return (true, hit, arPlane.isHorizontal ? 1 : 2);

					}
				}
			}
		} else {
		}
		return (false, hit, 0);
	}
	#endregion

}
