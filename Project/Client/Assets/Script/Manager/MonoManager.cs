using System;
using UnityEngine;

public class MonoManager : SingletonBehaviour<MonoManager>
{
    private void Start()
    {
        if (start__ != null) { start__.Invoke(this); }
    }
    public event Action<MonoManager> start
    {
        add { start__ += value; }
        remove { start__ -= value; }
    }
    Action<MonoManager> start__;

    private void Awake()
    {
        if (awake__ != null) { awake__.Invoke(this); }
    }
    public event Action<MonoManager> awake
    {
        add { awake__ += value; }
        remove { awake__ -= value; }
    }
    Action<MonoManager> awake__;

    private void OnEnable()
    {
        if (onEnable__ != null) { onEnable__.Invoke(this); }
    }
    public event Action<MonoManager> onEnable
    {
        add { onEnable__ += value; }
        remove { onEnable__ -= value; }
    }
    Action<MonoManager> onEnable__;

    private void OnDisable()
    {
        if (onDisable__ != null) { onDisable__.Invoke(this); }
    }
    public event Action<MonoManager> onDisable
    {
        add { onDisable__ += value; }
        remove { onDisable__ -= value; }
    }
    Action<MonoManager> onDisable__;

    private void FixedUpdate()
    {
        if (fixedUpdate__ != null) { fixedUpdate__.Invoke(this); }
    }
    public event Action<MonoManager> fixedUpdate
    {
        add { fixedUpdate__ += value; }
        remove { fixedUpdate__ -= value; }
    }
    Action<MonoManager> fixedUpdate__;

    private void Update()
    {
        if (update__ != null) { update__.Invoke(this); }
    }
    public event Action<MonoManager> update
    {
        add { update__ += value; }
        remove { update__ -= value; }
    }
    Action<MonoManager> update__;

    private void LateUpdate()
    {
        if (lateUpdate__ != null) { lateUpdate__.Invoke(this); }
    }
    public event Action<MonoManager> lateUpdate
    {
        add { lateUpdate__ += value; }
        remove { lateUpdate__ -= value; }
    }
    Action<MonoManager> lateUpdate__;

    private void OnDestroy()
    {
        if (onDisable__ != null) { onDisable__.Invoke(this); }
    }
    public event Action<MonoManager> onDestroy
    {
        add { onDestroy__ += value; }
        remove { onDestroy__ -= value; }
    }
    Action<MonoManager> onDestroy__;

    private void OnCollisionEnter(Collision collision)
    {
        if (onCollisionEnter__ != null) { onCollisionEnter__.Invoke(this, collision); }
    }
    public event Action<MonoManager, Collision> onCollisionEnter
    {
        add { onCollisionEnter__ += value; }
        remove { onCollisionEnter__ -= value; }
    }
    Action<MonoManager, Collision> onCollisionEnter__;

    private void OnCollisionExit(Collision collision)
    {
        if (onCollisionExit__ != null) { onCollisionExit__.Invoke(this, collision); }
    }
    public event Action<MonoManager, Collision> onCollisionExit
    {
        add { onCollisionExit__ += value; }
        remove { onCollisionExit__ -= value; }
    }
    Action<MonoManager, Collision> onCollisionExit__;

    private void OnCollisionStay(Collision collision)
    {
        if (onCollisionStay__ != null) { onCollisionStay__.Invoke(this, collision); }
    }
    public event Action<MonoManager, Collision> onCollisionStay
    {
        add { onCollisionStay__ += value; }
        remove { onCollisionStay__ -= value; }
    }
    Action<MonoManager, Collision> onCollisionStay__;

    private void OnMouseDown()
    {
        if (onMouseDown__ != null) { onMouseDown__.Invoke(this); }
    }
    public event Action<MonoManager> onMouseDown
    {
        add { onMouseDown__ += value; }
        remove { onMouseDown__ -= value; }
    }
    Action<MonoManager> onMouseDown__;

    private void OnMouseDrag()
    {
        if (onMouseDrag__ != null) { onMouseDrag__.Invoke(this); }
    }
    public event Action<MonoManager> onMouseDrag
    {
        add { onMouseDrag__ += value; }
        remove { onMouseDrag__ -= value; }
    }
    Action<MonoManager> onMouseDrag__;

    private void OnMouseUp()
    {
        if (onMouseUp__ != null) { onMouseUp__.Invoke(this); }
    }
    public event Action<MonoManager> onMouseUp
    {
        add { onMouseUp__ += value; }
        remove { onMouseUp__ -= value; }
    }
    Action<MonoManager> onMouseUp__;

    private void OnMouseEnter()
    {
        if (onMouseEnter__ != null) { onMouseEnter__.Invoke(this); }
    }
    public event Action<MonoManager> onMouseEnter
    {
        add { onMouseEnter__ += value; }
        remove { onMouseEnter__ -= value; }
    }
    Action<MonoManager> onMouseEnter__;

    private void OnMouseExit()
    {
        if (onMouseExit__ != null) { onMouseExit__.Invoke(this); }
    }
    public event Action<MonoManager> onMouseExit
    {
        add { onMouseExit__ += value; }
        remove { onMouseExit__ -= value; }
    }
    Action<MonoManager> onMouseExit__;

    private void OnMouseOver()
    {
        if (onMouseOver__ != null) { onMouseOver__.Invoke(this); }
    }
    public event Action<MonoManager> onMouseOver
    {
        add { onMouseOver__ += value; }
        remove { onMouseOver__ -= value; }
    }
    Action<MonoManager> onMouseOver__;
}

