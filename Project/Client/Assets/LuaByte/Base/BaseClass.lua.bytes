--保存类类型的虚表
local _class = {}
local a;
GLOBAL_OBJ_COUNT = {}
ENABLE_OBJ_COUNT = 0

function FindClassName(target, depth)
    for key, value in pairs(_G) do
        if value == target then
            return key
        end
    end
end

function ClasCountRetain(c)
    local key = FindClassName(c)
    if GLOBAL_OBJ_COUNT[key] == nil then
        GLOBAL_OBJ_COUNT[key] = 1
    else
        GLOBAL_OBJ_COUNT[key] = GLOBAL_OBJ_COUNT[key] + 1
    end
end

function ClasCountRelease(c)
    local key = FindClassName(c)
    if GLOBAL_OBJ_COUNT[key] == nil then
        --标识异常
        GLOBAL_OBJ_COUNT[key] = -100000
    else
        GLOBAL_OBJ_COUNT[key] = GLOBAL_OBJ_COUNT[key] - 1
    end
end

function PrintLuaClassCount(...)
    print("PrintLuaClassCount.............")
    for key, value in pairs(GLOBAL_OBJ_COUNT) do
        print("PrintLuaClassCount:" .. key .. ":", value)
    end
end

function BaseClass(super)
    -- 生成一个类类型
    local class_type = {}
    -- 在创建对象的时候自动调用
    class_type.__init = false
    class_type.__delete = false
    class_type.super = super

    class_type.New = function(...) --定义New成员方法
        -- 生成一个类对象
        local obj = {}
        obj._class_type = class_type

        -- 在初始化之前注册基类方法
        setmetatable(obj, {__index = _class[class_type]})

        -- 调用初始化方法
        do
            local create
            create = function(c, ...)
                if c.super then
                    create(c.super, ...) --对所有基类都进行init
                end
                if ENABLE_OBJ_COUNT ~= 0 then
                    ClasCountRetain(c)
                end
                if c.__init then
                    c.__init(obj, ...)
                end
            end

            create(class_type, ...)
        end

        -- 注册一个delete方法
        obj.DeleteMe = function(self)
            local now_super = self._class_type
            while now_super ~= nil do
                if ENABLE_OBJ_COUNT ~= 0 then
                    ClasCountRelease(now_super)
                end
                if now_super.__delete then
                    now_super.__delete(self) --对所有基类都进行delete
                end
                now_super = now_super.super
            end
        end

        return obj
    end

    local vtbl = {}
    _class[class_type] = vtbl

    setmetatable(
        class_type,
        {
            __newindex = function(t, k, v)
                vtbl[k] = v --赋值操作时self找不到的字段则对vtbl表赋值
            end,
            __index = vtbl --For call parent method
        }
    )

    if super then
        setmetatable(
            vtbl,
            {
                __index = function(t, k) --元表做“继承”操作
                    local ret = _class[super][k]
                    return ret
                end
            }
        )
    end

    return class_type
end
