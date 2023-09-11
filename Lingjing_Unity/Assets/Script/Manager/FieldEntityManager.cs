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
	private ARPlaneInfo arPlane;
	Queue<GameObject> queFieldEntity = new Queue<GameObject>();
	List<GameObject> queTaggedEntity = new List<GameObject>();
	Queue<GameObject> queLatLonEntity = new Queue<GameObject>();
	List<GameObject> lstPlacedEntity = new List<GameObject>();
	public GameObject[] arrPlacedEntity => lstPlacedEntity.ToArray();
	public bool isSurfacesReady => areaFloor + areaWall > 4;
	public bool isLoadFinish => queFieldEntity.Count == 0;
	public EntityActionManager actionManager;
	public FieldStageManager stageManager;
	public SceneLoadManager loadManager;
	public InputGPSManager geolocManager;

	private bool isPlaneVisible = false;
	private float timeLastCheck = 1f;

	private void Update() {
		goCamDir.transform.position = goCamera.transform.position;// + Vector3.down;
		Vector3 fwd = goCamera.transform.forward + goCamera.transform.up;
		fwd.y = 0;
		goCamDir.transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
		if (timeLastCheck < 0) {
			if (!loadManager.isLoading && loadManager.sceneMode == SceneLoadManager.SceneMode.AR) {
				if (!isLoadFinish) {
					TryPlaceRamdomEntities(10);
					//if (isLoadFinish) {
					//	loadManager.DebugLog("模型放置 " + (isLoadFinish ? "成功" : "失败"));
					//}
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
		foreach (GameObject entity in stageManager.lstFieldEntity) {
			if (PrepareEntity(entity)) {
				queFieldEntity.Enqueue(entity);
			}
		}
		foreach (GameObject entity in stageManager.lstTaggedEntity) {
			if (PrepareEntity(entity)) {
				queTaggedEntity.Add(entity);
			}
		}
		foreach (GameObject entity in stageManager.lstLocEntity) {
			if (PrepareEntity(entity)) {
				queLatLonEntity.Enqueue(entity);
			}
		}
	}

	private bool PrepareEntity(GameObject entity) {
		if (entity == null) {
			return false;
		}
		FieldEntityInfo info;
		if (entity.TryGetComponent(out info)) {
			info.entityManager = this;
			return true;
		}
		return false;
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
	}
	public void StopStage() {
		GameObject go;
		while (queFieldEntity.Count > 0) {
			go = queFieldEntity.Dequeue();
			if (go != null) {
				StopEntity(go);
			}
		}
		for (int i = queTaggedEntity.Count - 1; i >= 0; i--) {
			go = queTaggedEntity[i];
			queTaggedEntity.RemoveAt(i);
			if (go != null) {
				StopEntity(go);

			}
		}
		while (queLatLonEntity.Count > 0) {
			go = queLatLonEntity.Dequeue();
			if (go != null) {
				StopEntity(go);
			}
		}
		FieldEntityInfo info;
		for (int i = lstPlacedEntity.Count - 1; i >= 0; i--) {
			go = lstPlacedEntity[i];
			lstPlacedEntity.RemoveAt(i);
			if (go != null) {
				if (go.TryGetComponent(out info)) {
					info.RemoveFromField();
				} else {
					go.transform.parent = stageManager.goRoot.transform;
					go.transform.localPosition = Vector3.zero;
				}

				StopEntity(go);
			}
		}
	}

	private void StopEntity(GameObject entity) {
		if (entity == null) {
			return;
		}
		FieldEntityInfo info;
		if (entity.TryGetComponent(out info)) {
			if (OnEntityRemoved != null) {
				OnEntityRemoved.Invoke(info);
			}
		}
		//Destroy(entity);
	}

	//List<GameObject> geolocDir = new List<GameObject>();
	public void PlaceGeoLocEntities(int maxTries) {
		GameObject obj;
		GameObject goEntity;
		for (int i = 0; i < maxTries; i++) {
			if (queLatLonEntity.Count <= 0) {
				break;
			}

			goEntity = queLatLonEntity.Dequeue();
			if (goEntity.TryGetComponent(out FieldEntityInfo entityInfo)) {

				if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.GeoLocPosition) {
					obj = geolocManager.GetPosObject(entityInfo.latitude, entityInfo.longitude);
					entityInfo.goReference = obj;
					ForcePlacing(entityInfo);
				} else if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.GeoLocAround) {
					obj = geolocManager.GetPosObject(entityInfo.latitude, entityInfo.longitude);
					entityInfo.goReference = obj;
					queFieldEntity.Enqueue(goEntity);
				} else {
					queLatLonEntity.Enqueue(goEntity);
				}
			}
		}
	}
	public void PlaceGeoLocEntity() {
	}

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
		FieldEntityInfo entityInfo;
		if (queTaggedEntity.Count == 0) {
			return;
		}
		for (int i = queTaggedEntity.Count - 1; i >= 0; i--) {

			GameObject entity = queTaggedEntity[i];
			entityInfo = entity.GetComponent<FieldEntityInfo>();
			//Debug.Log(entityInfo.uuidImageTracking +" "+ i);
			if (entityInfo.uuidImageTracking == imgName) {

				if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.ARTagTracking) {
					//Debug.Log("queued item" + imgName);
					entity.transform.parent = imageTrackers[imgName].transform;
					entity.transform.localPosition = Vector3.zero;
					entity.transform.localRotation = Quaternion.identity;
					queTaggedEntity.RemoveAt(i);
					lstPlacedEntity.Add(entity);
					if (OnEntityPlaced != null) {
						OnEntityPlaced.Invoke(entityInfo);
					}
				} else if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.ARTagAround) {
					entityInfo.goReference = imageDirs[imgName];
					queTaggedEntity.RemoveAt(i);
					queFieldEntity.Enqueue(entity);
				} else if (entityInfo.enumARType == FieldEntityInfo.EntityToggleType.ARTagPosition) {
					entityInfo.goReference = imageDirs[imgName];
					ForcePlacing(entityInfo);
				}
			}
		}
	}

	private void ForcePlacing(FieldEntityInfo entityInfo) {
		Vector3 targetPos = entityInfo.goReference.transform.position + entityInfo.goReference.transform.rotation * entityInfo.offset;
		entityInfo.ForcePlacing(targetPos, entityInfo.goReference.transform.rotation, goRoot.transform);
		lstPlacedEntity.Add(entityInfo.gameObject);
		if (OnEntityPlaced != null) {
			OnEntityPlaced.Invoke(entityInfo);
		}
	}

	public GameObject GetStageImgDir() {

		if (stageManager.currentStage.stageToggleType == FieldStageInfo.StageToggleType.ARTag) {
			return imageDirs[stageManager.currentStage.uuidARTag];
		}
		return null;
	}

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
	private List<FieldEntityInfo> entityHorizontal = new List<FieldEntityInfo>();
	private List<FieldEntityInfo> entityVertical = new List<FieldEntityInfo>();
	//private Dictionary<ARPlaneInfo, List<FieldEntityInfo>> bookEntitiesOnPlane = new Dictionary<ARPlaneInfo, List<FieldEntityInfo>>();
	private int cntFloorEntities => entityHorizontal.Count;
	private int cntWallEntities => entityVertical.Count;

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
		FieldEntityInfo entity = arInfo as FieldEntityInfo;
		if (entity == null) {
			return;
		}
		if (entityHorizontal.Contains(entity)) {
			entityHorizontal.Remove(entity);
		} else if (entityVertical.Contains(entity)) {
			entityVertical.Remove(entity);
		}
		if (lstPlacedEntity.Contains(entity.gameObject)) {
			lstPlacedEntity.Remove(entity.gameObject);
		}
		stageManager.OnRemoveFieldEntity(entity.gameObject);
		if (OnEntityRemoved != null) {
			OnEntityRemoved.Invoke(entity);
		}
		//arSightManager.RemoveEntity(entity);
	}
	public void TryPlaceRamdomEntities(int maxTries) {
		GameObject goEntity;
		for (int i = 0; i < maxTries; i++) {
			if (queFieldEntity.Count <= 0) {
				//ARSceneScanHint.SetActive(false);
				break;
			}

			goEntity = queFieldEntity.Dequeue();
			if (goEntity.TryGetComponent(out FieldEntityInfo entityInfo)) {
				if (!entityInfo.goReference) {
					entityInfo.goReference = goCamDir;
				}
				if (TryPlaceEntityAround(entityInfo, entityInfo.goReference.transform.position, entityInfo.goReference.transform.rotation, 1f)) {
					if (OnEntityPlaced != null) {
						OnEntityPlaced.Invoke(entityInfo);
					}
					lstPlacedEntity.Add(goEntity);
				} else {
					queFieldEntity.Enqueue(goEntity);
				}
			}
		}

		if (isLoadFinish) {
			ARSceneScanHint.SetActive(false);
		} else if (!ARSceneScanHint.activeInHierarchy) {
			ARSceneScanHint.SetActive(true);
		}
	}
	public bool TryPlaceEntityAround(FieldEntityInfo entityInfo, Vector3 originPos, Quaternion originRot, float marginError) {
		//Debug.Log("tryPlaceEntity" + entityInfo.strName);
		Vector3 targetPos = originPos + originRot * entityInfo.offset;
		(bool hasPos, RaycastHit hit, int enumSurfaceType) = TryGetPos(targetPos, entityInfo.enumSurfaceType, marginError);
		if (hasPos) {
			if (enumSurfaceType == 1) {
				if (entityInfo.TryPlacing(hit.point, originRot, arPlane.gameObject, goRoot.transform)) {
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


}
