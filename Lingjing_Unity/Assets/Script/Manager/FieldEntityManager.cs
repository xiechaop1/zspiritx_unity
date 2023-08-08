using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldEntityManager : MonoBehaviour, IManager {
	//public static FieldEntityManager getInstance() {
	//	if (instance != null) {
	//		return instance;
	//	} else {
	//		GameObject go = GameObject.Find("FieldEntityManager");
	//		if (go != null && go.TryGetComponent(out instance)) {
	//			return instance;
	//		}
	//	}
	//	Debug.LogError("MISSING FieldEntityManager ");
	//	return null;
	//}
	//private static FieldEntityManager instance;

	//public ARSightManager arSightManager;
	public GameObject[] lstFieldEntity;
	public GameObject[] lstTaggedEntity;
	public GameObject goCamera;
	public GameObject goRoot;
	private ARPlaneInfo arPlane;
	Queue<GameObject> queFieldEntity = new Queue<GameObject>();
	List<GameObject> queTaggedEntity = new List<GameObject>();
	List<GameObject> lstPlacedEntity = new List<GameObject>();
	public bool isSurfacesReady => areaFloor + areaWall > 4;
	public bool isLoadFinish => queFieldEntity.Count == 0;
	//public delegate void OnEntityFound(FieldEntityInfo entityInfo);
	private EntityActionManager actionManager;

	private bool isPlaneVisible = false;

	public void Init(UIEventManager eventManager, params IManager[] managers) {
		//EntityActionManager entityActionManager=null;
		//foreach (var manager in managers) {
		//	if (manager is EntityActionManager) {
		//		actionManager = manager as EntityActionManager;
		//	}
		//}
		//arSightManager = ARSightManager.getInstance();
		//var eventManager = UIEventManager.getInstance();
		eventManager.RegisteredAction("FieldEntityManager", "RegisterARPlane", RegisterARPlane);
		eventManager.RegisteredAction("FieldEntityManager", "RemoveARPlane", RemoveARPlane);
		eventManager.RegisteredAction("FieldEntityManager", "RemoveFieldEntitys", RemoveFieldEntitys);
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
		//actionManager = entityActionManager;
	}
	public void RegisterManager(IManager manager) {
		if (manager is EntityActionManager) {
			actionManager = manager as EntityActionManager;
		}
	}
	public void PrepareScene(/*EntityActionManager actionManager*/) {
		GameObject obj;
		foreach (GameObject entity in lstFieldEntity) {
			if (PrepareEntity(entity, out obj)) {
				queFieldEntity.Enqueue(obj);
			}
		}
		foreach (GameObject entity in lstTaggedEntity) {
			if (PrepareEntity(entity, out obj)) {
				queTaggedEntity.Add(obj);
			}
		}
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
	private bool PrepareEntity(GameObject entity, out GameObject obj) {
		if (entity == null) {
			obj = null;
			return false;
		}
		FieldEntityInfo info;
		obj = Instantiate(entity, goRoot.transform);
		if (obj.TryGetComponent(out info)) {
			info.actionManager = actionManager;
			return true;
		}
		return false;
	}

	public void StopScene() {
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
		for (int i = lstPlacedEntity.Count - 1; i >= 0; i--) {
			go = lstPlacedEntity[i];
			lstPlacedEntity.RemoveAt(i);
			if (go != null) {
				StopEntity(go);
			}
		}
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
		Destroy(entity);
	}

	public GameObject PlaceImageTrackingEntity(string entityName) {
		GameObject obj = null;
		FieldEntityInfo entityInfo;
		foreach (GameObject entity in queTaggedEntity) {
			entityInfo = entity.GetComponent<FieldEntityInfo>();
			if (entityInfo.strName == entityName) {
				obj = entity;
				if (OnEntityPlaced != null) {
					OnEntityPlaced.Invoke(entityInfo);
				}
			}
		}
		if (obj != null) {
			queTaggedEntity.Remove(obj);
			lstPlacedEntity.Add(obj);
		}
		return obj;
	}

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
			ShiftOnNormal(entity);
		}
	}
	private void OnWallUpdate() {
		areaWall = CalculateArea(planeVertical);
		foreach (var entity in entityVertical) {
			ShiftOnNormal(entity);
		}
	}
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
				if (entity.TryPlacing(hit.point, entity.transform.rotation, hit.collider.gameObject, transform)) {

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
		if (OnEntityRemoved != null) {
			OnEntityRemoved.Invoke(entity);
		}
		//arSightManager.RemoveEntity(entity);
	}
	public void TryPlaceEntitys(int maxTries) {
		if (queFieldEntity.Count <= 0) return;

		GameObject goEntity;
		for (int i = 0; i < maxTries; i++) {
			if (queFieldEntity.Count <= 0) break;

			goEntity = queFieldEntity.Dequeue();
			FieldEntityInfo entityInfo;
			if (goEntity.TryGetComponent(out entityInfo)) {
				if (TryPlaceEntity(entityInfo)) {
					if (OnEntityPlaced != null) {
						OnEntityPlaced.Invoke(entityInfo);
					}
					lstPlacedEntity.Add(goEntity);
					//arSightManager.AddEntity(entityInfo);
				} else {
					queFieldEntity.Enqueue(goEntity);
				}
			}
		}


	}
	public bool TryPlaceEntity(FieldEntityInfo entityInfo) {
		Debug.Log("tryPlaceEntity" + entityInfo.strName);
		(bool hasPos, RaycastHit hit, int enumSurfaceType) = TryGetPos(goCamera.transform.position, entityInfo.enumSurfaceType);
		if (hasPos) {
			if (entityInfo.TryPlacing(hit.point, hit.collider.gameObject.transform.rotation, arPlane.gameObject, transform)) {
				if (enumSurfaceType == 1) {
					entityHorizontal.Add(entityInfo);
				} else if (enumSurfaceType == 2) {
					entityVertical.Add(entityInfo);
				}
				return true;
			}
		}

		return false;
	}

	(bool, RaycastHit, int) TryGetPos(Vector3 origin, int enumSurfaceType) {
		RaycastHit hit = new RaycastHit();
		//bool isHorizontal;
		if (enumSurfaceType == 1) {
			for (int i = 0; i < 50; i++) {
				//Debug.Log("tryGetPos horizontal, attempt: " + i);
				if (Physics.Raycast(origin + new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f)), Vector3.down, out hit, 4f, 8)) {
					if (hit.collider.gameObject.TryGetComponent(out arPlane) && arPlane.isHorizontal) {
						return (true, hit, 1);
					}
				}
			}
		} else if (enumSurfaceType == 2) {
			Ray ray;
			for (int i = 0; i < 50; i++) {
				//Debug.Log("tryGetPos omni, attempt: " + i);
				if (Physics.Raycast(origin, new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f)).normalized, out hit, 4f, 8)) {
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
				if (Physics.Raycast(origin, new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f)).normalized, out hit, 4f, 8)) {
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
