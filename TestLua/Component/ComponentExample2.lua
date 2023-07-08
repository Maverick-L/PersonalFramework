---@class ComponentExample2 : ComponentPublic
---@field New function
---@field Log function
---@field Log2 function
---@field Log3 function
---@class ComponentExamplePrivate : ComponentPrivate
--全局静态变量
local public = ComponentExample2 --public
--全局私有变量
local privateClass =ComponentExample2Private --私有

public.aaa =333

function public:Init()
    --公共变量命名规则
    self.sssss =333333
    print("example2 adress:"..tostring(self))
end

function public:Log(log1)
    print("Example2  Log: ".."\t"..log1)
end
function public:Log2()
end

function public:Log3(value)
    print("Example2 Log3:\t"..value.."\t"..self.sssss)
end

function privateClass:LogA(instance)
    --私有变量命名规则
    self.sss =2222
    self.publicC:Log2()
end



--公共全局静态方法调用
--内部:public:Func()
--外部: obj:Func()
--注意，不可以使用“.”方式调用public方法

--公共全局静态方法创建规则
-- function public:Func()
-- or
-- function public.Func(instance)

--公共方法调用和创建规则同上
--公共方法中的的“self”  = obj的实例

--私有方法
--在public方法中调用规则:self(obj的实例).privateC:Func()
--在private方法中调用规则:self:Func()
--调用公共方法:self.publicC:Func()
--私有方法中的“self” = obj的private实例

--全局私有方法调用:
--privateClass:Func()
-- or
-- privateClass.Func()
--使用全局私有方法调用其他方法时候需要在当前全局方法中传入"obj"