using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aAV_Copyright : MonoBehaviour
{	
	
	void OnWillRenderObject()
	{
		string name = this.name;
		if(name.Contains("terrain")){
			string tNo = name.Trim().Substring(name.Trim().Length - 2);
			if(tNo == "00"){
				aAV_Public.copyright += checkCopyright(aAV_Public.basicInfo.copyright_N);
			}else{
				aAV_Public.copyright += checkCopyright(aAV_Public.basicInfo.copyright_W);
			}
		}else{
			string pNo = this.transform.parent.gameObject.name.Remove(0,7);
			aAV_Public.copyright += checkCopyright(aAV_Public.datalist[int.Parse(pNo)-1].copyright);
		}
	}
	
	string checkCopyright(string text){
		string copy = "";
		if(!aAV_Public.copyright.Contains(text)){
			if(aAV_Public.copyright !=""){
				copy = ", ";
			}
			copy += text;
		}
		return copy;
	}
}
