function main()
    require("class")
    require("unityClass")

    local obj1 = GameObject.Find("GameObject1")
    local obj2 = GameObject.Instantiate(obj1)
    obj2.name = "helloworld"
end
