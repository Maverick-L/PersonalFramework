local _rootPath = "Component/%s"

local privateFunc ={}

function CreateComponent(name,...)
    if _G[name] == nil then
        privateFunc:NewOOPClass(name)
    end
    local obj = _G[name].New(...)
    if obj.Regist then
        obj.Regist()
    end
    return obj
end

function DestroyComponent(obj)
    if obj.UnRegist then
        obj.UnRegist()
    end
    obj.Destroy()
end
---require Component
function privateFunc:CreateComponent(name)
    _G[name] = Util.Class(name)
    _G[name.."Private"] = Util.Class(name.."Private")
    require(_rootPath:format(name))
    if _G[name].New == nil then
        error("Component:"..name.."没有New方法")
    end
end

--创建oop的元表
function privateFunc:NewOOPClass(name)
    --全局公共变量
    local staticPublic ={}
    --全局私有变量
    local staticPrivate ={}
    _G[name] = setmetatable({},{})
    _G[name.."Private"] = setmetatable({},{})

    require(_rootPath:format(name))
    --获取配置的全局变量内容
    for key,value in pairs(_G[name]) do
        if type(value) ~= "function" then
            staticPublic[key] =value
            _G[name][key] = nil    
        end
    end
    for key,value in pairs(_G[name.."Private"]) do
        if type(value) ~= "function" then
            staticPrivate[key] =value
            _G[name.."Private"][key] = nil    
        end
    end
    local baseName =staticPrivate.base
    if baseName ~= nil and _G[baseName] == nil then
        self:NewOOPClass(baseName)
        setmetatable(_G[name],_G[baseName])
    end
    local New = function(...)
        local instance = setmetatable({},_G[name])
        instance.name = name
        local private = setmetatable({},_G[name.."Private"])
        instance.private = private
        private.public = instance
        local table ={
            __index = function(table,key) 
                if key == "private" then
                    error("无法访问private变量")
                end
                if instance[key] then
                    if type(instance[key]) ~= "function" then
                        return instance[key]
                    else
                        return function(...) 
                            instance[key](instance,...)
                        end
                    end
                end
                return rawget(table,key)
            end

        }
        if instance.New then
            instance:New(...)
        end
        instance.Destroy = function() 
            instance = nil
            private = nil
            table = nil
        end
        return table
    end
     getmetatable(_G[name]).__index = function(table,key) 
        if staticPublic[key] then
            return staticPublic[key]
        else
            return rawget(table,key)
        end
     end
     getmetatable(_G[name]).New = New
end



---@class ComponentPublic
---@field view ViewBase
---@field privateC ComponentPrivate 
---@field RegistEvent function 注册消息内容
---@field UnRegistEvent function  取消注册
---@field Destroy function 删除
---@field baseObj ComponentPublic
---@field Init function 初始化

---@class ComponentPrivate
---@field publicC ComponentPublic
---@field base EComponent

function TestComponent()
    local self = {}
    local com1 = CreateComponent(self,"ComponentExample",nil) ---@type ComponentExample
    com1:Log(1111)
    com1:Log2(22222)
    com1:Log3(3333)
end


Util = require("Util")
EComponent = require("EComponent")
TestComponent()