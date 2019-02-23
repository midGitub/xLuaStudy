GameManager = {}
local this = GameManager

-- 将View的脚本一一注册进去
function GameManager.InitViews()
    require("UIManager")

    require("UIStartView")
end

--初始化方法 ctrl 和 view 同时加载root界面
function GameManager.Init()
    this.InitViews()

    UIManager.LoadUi("UIStartView")
end
