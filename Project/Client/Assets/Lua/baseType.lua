baseType = class()
function baseType:ctor(x) -- 定义 base_type 的构造函数
    print("base_type ctor")
    self.x = x
end

function baseType:print_x() -- 定义一个成员函数 base_type:print_x
    print(self.x)
end

function baseType:hello() -- 定义另一个成员函数 base_type:hello
    print("hello base_type")
end
