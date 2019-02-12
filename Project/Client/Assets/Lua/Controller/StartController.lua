require("UIStartView")

StartController = StartController or BaseClass()

function StartController:__init()
    if StartController.Instance ~= nil then
        logError("StartController.Instance has init twin")
        return
    end
    StartController.Instance = self

    self.view = UIStartView.New()
    print("实例化   uistartView")
end
