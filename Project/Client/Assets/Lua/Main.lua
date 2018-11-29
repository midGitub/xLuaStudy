require("GameInit")

function main()
    --GameInit.Init()
    local ClassA = class("classA")

    function ClassA:ctor( ... )
    
        print("ClassA:ctor params is ", ...)
        self.name = ...
    end
    
    function ClassA:sayHello()
    
        print(string.format("my name is %s", self.name))
    end
    
    local classA_1 = ClassA:create("ClassA_1")
    classA_1:sayHello()
    print("=============================")
    local classA_2 = ClassA:create("ClassA_2")
    classA_2:sayHello()
    print("=============================")
    classA_1.name = "XXX"
    classA_1:sayHello()
    classA_2:sayHello()
end

 class = function (className)

    if type(className) ~= "string" then
        error("class name must string type")
    end
    local cls = {__className = className}
    cls.ctor = function ( ... ) 
        -- default constructor
    end
    cls.new = function ( ... )
        local instance = {}
        setmetatable(instance, {__index = cls})
        instance.__class = cls
        instance.ctor(...)
        return instance
    end
    cls.create = function ( ... )
        return cls.new(...) 
    end
    return cls
end