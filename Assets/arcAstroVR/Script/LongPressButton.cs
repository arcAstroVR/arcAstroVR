//  LongPressButton.cs
//  http://kan-kikuchi.hatenablog.com/entry/LongPressButton
//
//  Created by kan.kikuchi on 2016.04.18.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

/// <summary>
/// ボタンを長押しすると、連続でメソッドを実行出来るようにするクラス
/// </summary>
public class LongPressButton : MonoBehaviour {

  //実行するメソッド
  [SerializeField]
  UnityEngine.Events.UnityEvent _event;

  //長押しと判定する時間、メソッドを実行する間隔
  [SerializeField]
  private float _longPressTime = 1, _invokeInterval = 0.2f;

  //長押しと判定するまで or 次のメソッドを実行するまでの時間
  private float _waitTime = 0;

  //押しているか
  private bool _isPressing = false;

  //一度でもメソッドを実行したか
  private bool _isInvokedEvent = false;

  //=================================================================================
  //初期化
  //=================================================================================

  private void Awake (){
    //ボタンを押し時のイベント作成
    EventTrigger.Entry pressDown = new EventTrigger.Entry();
    pressDown.eventID = EventTriggerType.PointerDown;
    pressDown.callback.AddListener((data)=>{PressDown();});

    //ボタンを離した時のイベント作成
    EventTrigger.Entry pressUp = new EventTrigger.Entry();
    pressUp.eventID = EventTriggerType.PointerUp;
    pressUp.callback.AddListener((data)=>{PressUp();});

    //ボタンをクリックした時のイベント作成
    EventTrigger.Entry click = new EventTrigger.Entry();
    click.eventID = EventTriggerType.PointerClick;
    click.callback.AddListener((data)=>{Click();});

    //イベントトリガーを追加し、イベントを登録
    EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
    trigger.triggers.Add(pressDown);
    trigger.triggers.Add(pressUp);
    trigger.triggers.Add(click);
  }

  //=================================================================================
  //イベント発生時に実行されるメソッド
  //=================================================================================

  //ボタンを押した瞬間に実行されるメソッド
  private void PressDown(){
    _isPressing     = true;
    _isInvokedEvent = false;
    _waitTime       = _longPressTime;
  }

  //ボタンを離した瞬間に実行されるメソッド
  private void PressUp(){
    _isPressing = false;
  }

  //クリックした瞬間に実行されるメソッド
  private void Click(){
    //一度もイベントが実行されていなければ実行
    if(!_isInvokedEvent){
      _event.Invoke();
    }
  }

  //=================================================================================
  //更新
  //=================================================================================

  private void Update(){
    //ボタンが押されていない時はスルー
    if(!_isPressing){
      return;
    }

    //待ち時間を減らす
    _waitTime -= Time.deltaTime;

    //待ち時間がまだある時はスルー
    if(_waitTime > 0){
      return;
    }

    //メソッド実行、待ち時間設定
    _event.Invoke();
    _waitTime = _invokeInterval;
    _isInvokedEvent = true;
  }

}  
