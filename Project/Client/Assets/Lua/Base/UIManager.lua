

UIManager = {}
local this = UIManager

local viewList = {}

function UIManager.LoadUi(name)
    CS.LuaHelperManager.Instance:LoadUI(name, this.OnCreate)
end

function UIManager.OnCreate(name, go, tab)
    viewList[name] = go
    print(tab.c)
    tab.test()
    --print(tab.UIType);
    -- print(uiTable.name)
    --uiTable.test()
end

function UIManager.FindUiByName(name)
    return viewList[name]
end
