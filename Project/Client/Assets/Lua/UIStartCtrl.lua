UIStartCtrl = {}
local this = UIStartCtrl

local root
local transform
local gameobject

function UIStartCtrl.New()
    return this
end

function UIStartCtrl.Awake()
    print("主界面、启动了")

    --这里的方法就是负责把UI给克隆出来  遇到方法时 使用 : 加上后面的方法
    CS.LuaHelperManager.Instance:LoadUI("UIStartView", this.OnCreate)
    --CS.LuaHelperManager 去调用Unity中的LuaHelperManager脚本中的方法加载UI及其回调
end

-- Awake加载UI的回调（返回克隆的obj对象）
function UIStartCtrl.OnCreate(obj)
    print("UI克隆完毕的回调")


end



---------------------
