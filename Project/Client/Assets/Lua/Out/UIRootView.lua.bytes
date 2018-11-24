UIRootView = {}
local this = UIRootView

local transform  -- 自身的transform引用
local gameobject  -- 自身的gameobject引用

function UIRootView.Awake(obj)
    gameobject = obj
    transform = obj.transform
    this.Init()
end

--初始化面板  找到UI组件
function UIRootView.Init()
    print("初始化面板  找到UI组件")
    --寻找组件
    this.btnOpenTask = transform:FindChild("btnOpenTask"):GetComponent("UnityEngine.UI.Button")

    this.btnOpenTask.onClick:AddListener(UIRootCtrl.OnBtnOpenTaskClick)
end

function UIRootView.Start()
end

function UIRootView.Update()
end

function UIRootView.OnDestroy()
    this.btnOpenTask.onClick = null
end

-- 按钮点击的方法
function UIRootCtrl.OnBtnOpenTaskClick()
    print("点击事件(热更)")
end
---------------------
