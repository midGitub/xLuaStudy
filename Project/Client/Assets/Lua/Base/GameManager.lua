require "CtrlMgr"
require "CtrlNames"
 -- CtrlMgr.lua  管理控制器脚本、里面table 表存储其名字及其对应的ctrl脚本的实例 --
GameManager = {}
local this = GameManager
local c
-- 将View的脚本一一注册进去
function GameManager.InitViews()
        require("UIStartView")
end

--初始化方法 ctrl 和 view 同时加载root界面
function GameManager.Init()
    this.InitViews()
    CtrlMgr.Init()
    GameManager.loadView(CtrlNames.UIStartCtrl)
end

--从控制器中拿出一个来调用它的Awake方法 加载窗体--
function GameManager.loadView(type)
    local ctrl = CtrlMgr.GetCtrl(type)
    if ctrl ~= nil then
        ctrl.Awake()
    end
end
--------------------- 