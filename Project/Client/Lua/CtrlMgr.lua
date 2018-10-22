require "Define"

--注册UI控制器 （动态添加）------------------
require "UIRootCtrl"

CtrlMgr = {}

local this = CtrlMgr

local ctrlList = {}

--初始化方法 往列表中添加所有的控制器
function CtrlMgr.Init()
    -- 注册控制器到table表中（动态添加）------------------
    ctrlList[CtrlNames.UIRootCtrl] = UIRootCtrl.New()

    return this
end

--根据控制器的名称 获取控制器
function CtrlMgr.GetCtrl(ctrlName)
    return ctrlList[ctrlName]
end
---------------------
