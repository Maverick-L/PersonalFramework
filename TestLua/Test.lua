local class = Test
local data = "我是test"

class.mydata = "我是mydata"
function class:ToString()
    print(data)
end