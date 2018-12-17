UIStartView = {}
local this = UIStartView

local transform
local gameObject
local c;
function UIStartView.Awake(obj)
    gameObject = obj
    transform = obj.transform
    this.Init()
end

function UIStartView.Init()
    print("初始化uistartview")

    this.startButton = transform:FindChild("ButtonGrid/StartButton"):GetComponent("UnityEngine.UI.Button")
    this.startButton.onClick:AddListener(this.OnStartButtonClick)
end

function UIStartView.Start()
end

function UIStartView.Update()
end

function UIStartView.OnDestroy()
    this.startButton.onClick = null
    this.startButton = null;
    print("UIStartView.OnDestroy()")
end

function UIStartView.OnStartButtonClick()
    print("OnStartButtonClick()")
end
