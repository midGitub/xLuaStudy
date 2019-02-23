ModuleManager = ModuleManager or BaseClass()
function ModuleManager:__init()
	if ModuleManager.Instance ~= nil then
		logError("ModuleManager.Instance has init twin")
		return
	end
	ModuleManager.Instance = self

	self.ctrl_list = {}

	self:CreateController()

	--StartController.Instance:StartLogin()
end

function ModuleManager:__delete()
	self:DeleteController()
end

function ModuleManager:CreateController()
	table.insert(self.ctrl_list, StartController.New())
end

function ModuleManager:DeleteController()
	for key, ctrl in ipairs(self.ctrl_list) do
		ctrl:DeleteMe()
	end
	self.ctrl_list = nil
end
