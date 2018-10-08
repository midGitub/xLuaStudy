test = class(baseType) -- 定义一个类 test 继承于 base_type

function test:ctor() -- 定义 test 的构造函数
    print("test ctor")
end

function test:hello() -- 重载 base_type:hello 为 test:hello
    print("hello test")
end
