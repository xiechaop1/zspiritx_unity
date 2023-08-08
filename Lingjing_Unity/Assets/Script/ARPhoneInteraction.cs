using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (FieldEntityInfo))]
public class ARPhoneInteraction : ARInteractListener {
	public Text txtDisplay;
	private string phoneNum;
	private EntityActionManager actionManager;

	private void Start() {
		actionManager = GetComponent<FieldEntityInfo>().actionManager;//EntityActionManager.getInstance();
	}
	public override void HasARInteract(string info) {
		int num;
		if (int.TryParse(info, out num)) {
			if (num >= 0 && num <= 9) {
				AddNum2PhoneNum(num);
			}
		} else if (info == "DELETE") {
			RestPhoneNum();
		} else if (info == "ENTER") {
			if (string.IsNullOrWhiteSpace(phoneNum)) {
				actionManager.UpdateHintText("请先输入电话号码再拨打");
			} else {
				StartCoroutine(CallNumber());
			}
		}
	}
	private void AddNum2PhoneNum(int num) {
		phoneNum += num;
		txtDisplay.text = phoneNum;
	}
	private void RestPhoneNum() {
		phoneNum = "";
		txtDisplay.text = phoneNum;
	}
	private IEnumerator CallNumber(){ 
		actionManager.UpdateHintText("已拨打电话:" + phoneNum + "\n正在等待接通……");
		yield return new WaitForSeconds(2);
		if (phoneNum=="1527") {
			actionManager.UpdateHintText("已拨打电话:" + phoneNum + "\n正在等待接通……\n\n恭喜你，找到了正确的号码");
		}else{ 
			actionManager.UpdateHintText("已拨打电话:" + phoneNum + "\n正在等待接通……\n\n对不起，您拨打的是空号");
		}

	}
}
