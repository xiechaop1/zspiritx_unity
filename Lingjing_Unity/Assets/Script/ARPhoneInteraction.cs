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
				actionManager.UpdateHintText("��������绰�����ٲ���");
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
		actionManager.UpdateHintText("�Ѳ���绰:" + phoneNum + "\n���ڵȴ���ͨ����");
		yield return new WaitForSeconds(2);
		if (phoneNum=="1527") {
			actionManager.UpdateHintText("�Ѳ���绰:" + phoneNum + "\n���ڵȴ���ͨ����\n\n��ϲ�㣬�ҵ�����ȷ�ĺ���");
		}else{ 
			actionManager.UpdateHintText("�Ѳ���绰:" + phoneNum + "\n���ڵȴ���ͨ����\n\n�Բ�����������ǿպ�");
		}

	}
}
