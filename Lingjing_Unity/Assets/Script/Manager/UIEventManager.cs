using System.Collections;

using System;
using System.Collections.Generic;
using UnityEngine;


public class UIEventManager : MonoBehaviour, IManager {
	public static UIEventManager getInstance() {
		if (instance != null) {
			return instance;
		} else {
			GameObject go = GameObject.Find("UIEventManager");
			if (go != null && go.TryGetComponent(out instance)) {
				DontDestroyOnLoad(go);
				return instance;
			}
		}
		Debug.LogError("MISSING UIEventManager ");
		return null;
	}
	private static UIEventManager instance;
	public void Init(UIEventManager eventManager, params IManager[] managers) {

	}

	public static void CallEvent(string managerName, string actionName) {
		instance.CallEventOnManager(managerName, actionName);
	}
	public static void CallEvent(string managerName, string actionName, IEventMessage arInfo) {
		instance.CallEventOnManager(managerName, actionName, arInfo);
	}
	public static void BroadcastEvent(string actionName, string info) {
		instance.BroadcastEventByName(actionName, info);
	}


	Dictionary<string, Dictionary<string, Action>> bookEventbyAddress = new Dictionary<string, Dictionary<string, Action>>();

	Dictionary<string, Dictionary<string, Action<IEventMessage>>> bookActionbyAddress = new Dictionary<string, Dictionary<string, Action<IEventMessage>>>();

	Dictionary<string, List<Action<string>>> bookBroadcastbyAddress = new Dictionary<string, List<Action<string>>>();

	public void RegisteredAction(string managerName, string actionName, Action action) {
		Dictionary<string, Action> bookEventAtManager;
		if (bookEventbyAddress.ContainsKey(managerName)) {
			bookEventAtManager = bookEventbyAddress[managerName];
		} else {
			bookEventAtManager = new Dictionary<string, Action>();
			bookEventbyAddress.Add(managerName, bookEventAtManager);
		}
		bookEventAtManager.Add(actionName, action);
	}
	public void RegisteredAction(string managerName, string actionName, Action<IEventMessage> action) {
		Dictionary<string, Action<IEventMessage>> bookEventAtManager;
		if (bookActionbyAddress.ContainsKey(managerName)) {
			bookEventAtManager = bookActionbyAddress[managerName];
		} else {
			bookEventAtManager = new Dictionary<string, Action<IEventMessage>>();
			bookActionbyAddress.Add(managerName, bookEventAtManager);
		}
		bookEventAtManager.Add(actionName, action);
	}

	public void SubscribeBroadcast(string actionName, Action<string> action) {
		List<Action<string>> broadcast;
		if (bookBroadcastbyAddress.ContainsKey(actionName)) {
			broadcast = bookBroadcastbyAddress[actionName];
		} else {
			broadcast = new List<Action<string>>();
			bookBroadcastbyAddress.Add(actionName, broadcast);
		}
		broadcast.Add(action);
	}

	private void CallEventOnManager(string managerName, string actionName) {
		Dictionary<string, Action> bookEventAtManager;
		if (bookEventbyAddress.TryGetValue(managerName, out bookEventAtManager)) {
			bookEventAtManager[actionName]?.Invoke();
		}
	}

	private void CallEventOnManager(string managerName, string actionName, IEventMessage arInfo) {
		Dictionary<string, Action<IEventMessage>> bookEventAtManager;
		if (bookActionbyAddress.TryGetValue(managerName, out bookEventAtManager)) {
			bookEventAtManager[actionName]?.Invoke(arInfo);
		}
	}

	private void BroadcastEventByName(string actionName, string info) {
		List<Action<string>> broadcast;
		if (bookBroadcastbyAddress.TryGetValue(actionName, out broadcast)) {
			foreach (var call in broadcast) {
				call?.Invoke(info);
			}
		}
	}

}
public interface IEventMessage {
}
public interface IManager {
	void Init(UIEventManager eventManager, params IManager[] managers);
}

