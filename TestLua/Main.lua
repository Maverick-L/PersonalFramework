local class = {}
local Const ={}
Const.MiniGameLinkFindDir ={
    None =1,
    Left =2,
    Right =3,
    Up = 4,
    Down = 5
}
function table.tostring(tab)
    local typeTab = type({})
    local typeStr = type("")
    local str = "{"
    for k, v in pairs(tab) do
        local kStr = nil
        local vStr = nil

        if type(k) == typeTab then
            kStr = table.tostring(k)
        elseif type(k) == typeStr then
            kStr = string.format("\"%s\"", k)
        else
            kStr = tostring(k)
        end

        if type(v) == typeTab then
            vStr = table.tostring(v)
        elseif type(v) ==  typeStr then
            vStr = string.format("\"%s\"", v)
        else
            vStr = tostring(v)
        end

        str = string.format("%s %s:%s,", str, kStr, vStr)
    end

    str = str .. "}"

    return str
end
function table.deepCopy(object)
    local lookup_table = {}
    local function _copy(object)
        if type(object) ~= "table" then
            return object
        elseif lookup_table[object] then
            return lookup_table[object]
        end

        local new_table = {}

        lookup_table[object] = new_table
        for key, value in pairs(object) do
            new_table[_copy(key)] = _copy(value)
        end
        return setmetatable(new_table, getmetatable(object))
    end

    return _copy(object)
end


function class:OnInitOther()
    --测试数据
    self.allInitPictrue ={
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10",
        "11",
        "12",
        "13",
        "14",
        "15",
        "16",
        "17",
        "18",
        "19",
        "20",
        "21",
        "22",
        "23",
        "24",
    }


end

function class:OnGameStartInit()
    self.finshList = {}  ---@type CardInfo[] 已经连完的数组
    self.cardMap ={} ---@type CardInfo[]  二维数组，用来模拟点击
    self.lineCount = 6 --行数
    self.rowCount =8  --列数
    for i=1,#self.allInitPictrue * 2 do
        table.insert(self.cardMap,{})
    end
end

function class:OnGameRealStart()
    self:RefreshAllCard(true)
    self.cardMapToDouble ={}
    --创建一个比已有数据大一圈的二维数组，方便寻路
    local cardMap = table.deepCopy(self.cardMap)

    self.cardMap ={}
    for i=1,self.lineCount+2 do
        table.insert(self.cardMapToDouble,{})
    end
    for i=1,self.rowCount+2 do
        table.insert(self.cardMap,{isFinsh=true,line=1,row=i})
        table.insert(self.cardMapToDouble[1],i)
    end
    local index =1
    for i=1,self.lineCount do
        table.insert(self.cardMap,{isFinsh = true,line=i+1,row=1})
        table.insert(self.cardMapToDouble[i+1],#self.cardMap)
        for j=1,self.rowCount do
           -- local info = cardMap[index]
            local picIndex = index % #self.allInitPictrue == 0 and #self.allInitPictrue or index %#self.allInitPictrue
            local info = {line = 0,row=0,isFinsh=false,target = index+#self.allInitPictrue,pic =self.allInitPictrue[picIndex]}
            info.line =(i+1)
            info.row =j+1
            local realTarget = info.target + math.floor(info.target /(self.rowCount))*2+(self.rowCount+2)+1
            if info.target % self.rowCount == 0 then
                realTarget = realTarget-2
            end
            info.target =realTarget
            table.insert(self.cardMap,info)
            table.insert(self.cardMapToDouble[i+1],#self.cardMap)
            index = index+1
        end
        table.insert(self.cardMap,{isFinsh = true,line =i+1,row=self.rowCount+2})
        table.insert(self.cardMapToDouble[i+1],#self.cardMap)
    end
    for i=1,self.rowCount+2 do
        table.insert(self.cardMap,{isFinsh=true,line=#self.cardMapToDouble,row=i})
        table.insert(self.cardMapToDouble[#self.cardMapToDouble],#self.cardMap)
    end
    for i=1,#self.cardMapToDouble do
        local str =""
        for j=1,#self.cardMapToDouble[i] do
            local cardInfo = self.cardMap[self.cardMapToDouble[i][j]]
            local pos = cardInfo.row == nil and "nil" or "("..cardInfo.line..","..cardInfo.row..")"
            str = str .."\t"..self.cardMapToDouble[i][j]..":"..pos..":"..tostring(cardInfo.target)..":"..tostring(cardInfo.pic)
        end
        print(str)
    end
end



function class:OnGameStateChange()

end

function class:CheckCanLink(point1,point2)
    local card1 = self.cardMap[point1]
    local card2 = self.cardMap[point2]
    print(tostring(card1.target))
    if card1.target ~= point2 then
        return false
    end
    print("startIndex:"..point1.."\tendIndex:"..point2)
    local isCanMove,roadList = self:FindRoad(Const.MiniGameLinkFindDir.None,1,point1,point2,card1.line,card1.row)
    if isCanMove then
        self.roadList = roadList
        --开始下一步

        table.sort(self.roadList,function(a, b)
            return a[1]>b[1]
        end)
        local str =""
        for i=1,#self.roadList do
            local info =self.roadList[i]
            str = str .."\tindex:"..info[1].."\tpoint1:"..point1.."\tpoint2:"..point2
        end
    end
    return isCanMove
end

function class:FindRoad(lastDir,nowLine,startPoint,targetPoint,line,row)
    local pointIndex = self.cardMapToDouble[line][row]
    if pointIndex == targetPoint then
        return true ,{{targetPoint,lastDir}}
    end
    if pointIndex ~= startPoint  and not self:CheckPointCanMove(line,row) then
        return false
    end
    local dirList = self:CalcuFindDir(self.cardMap[pointIndex],self.cardMap[targetPoint],nowLine,lastDir)
    if dirList == nil or #dirList == 0 then
        return false
    end
    for i=1,#dirList do
        local canMove, nextLine,nextRow = self:CalcuNextMovePoint(dirList[i],line,row)
        if canMove then
            local lineCount = (dirList[i] == lastDir or lastDir == Const.MiniGameLinkFindDir.None) and nowLine or nowLine+1
            local finsh,road = self:FindRoad(dirList[i],lineCount,startPoint,targetPoint,nextLine,nextRow)
            if finsh then
                table.insert(road,{pointIndex,lastDir})
                return finsh,road
            end
        end
    end
    if nowLine <=2 and not table.contains(dirList,lastDir) then
        if lastDir ~= Const.MiniGameLinkFindDir.None then
            local canMove,nextLine,nextRow = self:CalcuNextMovePoint(lastDir,line,row)
            if canMove then
                local finsh,road = self:FindRoad(lastDir,nowLine,startPoint,targetPoint,nextLine,nextRow)
                if finsh then
                    table.insert(road,{pointIndex,lastDir})
                    return finsh,road
                end
            end
        else
            for key,dir in pairs(Const.MiniGameLinkFindDir) do
                if not table.contains(dirList,dir) and dir ~= Const.MiniGameLinkFindDir.None then
                    local canMove,nextLine,nextRow = self:CalcuNextMovePoint(dir,line,row)

                    if canMove then
                        local finsh,road = self:FindRoad(dir,nowLine,startPoint,targetPoint,nextLine,nextRow)
                        if finsh then
                            table.insert(road,{pointIndex,dir})
                            return finsh,road
                        end
                    end
                end
            end
        end
    end
    return false
end


function class:CalcuNextMovePoint(dir,nowLine,nowRow)
    local nextLine,nextRow
    if dir == Const.MiniGameLinkFindDir.Right then
        nextLine,nextRow = nowLine,nowRow+1
    end
    if dir == Const.MiniGameLinkFindDir.Left then
        nextLine,nextRow = nowLine,nowRow-1
    end
    if dir == Const.MiniGameLinkFindDir.Up then
        nextLine,nextRow = nowLine-1,nowRow
    end
    if dir == Const.MiniGameLinkFindDir.Down then
        nextLine,nextRow = nowLine+1,nowRow
    end
    local canMove =  not(nextLine == 0 or nextLine >self.lineCount+2 or nextRow == 0 or nextRow > self.rowCount+2)
    return canMove,nextLine,nextRow
end

function class:CheckPointCanMove(line,row)
    local pointIndex = self.cardMapToDouble[line][row]
    return  self.cardMap[pointIndex].isFinsh
end

---@param card1 CardInfo
---@param card2 CardInfo
function class:CalcuFindDir(card1,card2,lineCount,lastDir)
    local dirLineCount = card1.line - card2.line
    local dirRowCount = card1.row - card2.row
    if lineCount == 3 and dirLineCount ~= 0 and dirRowCount ~= 0 then
        return nil
    end
    local lineDir = dirRowCount > 0 and Const.MiniGameLinkFindDir.Left or Const.MiniGameLinkFindDir.Right
    local rowDir =dirLineCount > 0 and Const.MiniGameLinkFindDir.Up or Const.MiniGameLinkFindDir.Down
    local dirList ={}
    if lineCount ==3 then
        if dirLineCount ~= 0 and rowDir == lastDir then
            table.insert( dirList,rowDir)
        elseif dirRowCount ~= 0 and lineDir == lastDir then
            table.insert( dirList,lineDir)
        else
            return nil
        end
    elseif lineCount ==2 then
        if lineDir == lastDir  then
            if dirRowCount == 0 then
                table.insert( dirList,rowDir)
            end
            table.insert( dirList,lineDir)

        elseif rowDir == lastDir then
            if dirLineCount == 0 then
                table.insert( dirList,lineDir)
            end
            table.insert( dirList,rowDir)
        else
            return nil
        end
    else
        dirList = { lineDir,rowDir }
    end
    local notMove
    if lastDir == Const.MiniGameLinkFindDir.Down or lastDir == Const.MiniGameLinkFindDir.Right then
        notMove =lastDir-1
    elseif lastDir == Const.MiniGameLinkFindDir.Left or lastDir == Const.MiniGameLinkFindDir.Left then
        notMove = lastDir +1
    end
    local index = table.findIndex(dirList,notMove)
    if index ~= nil then
        table.remove(dirList,index)
    end
    return dirList
end

function class:RefreshAllCard(isAll)
    local refreshMap ={}
    local emptyIndexList ={}
    if not isAll then
        for i=1,#self.finshList do
            if not self.finshList.isFinsh then
                table.insert(emptyIndexList,i)
            else
                table.insert(self.finshList[i].pic)
            end
        end
    else
        refreshMap =table.deepCopy(self.allInitPictrue)
        for i=1,#self.allInitPictrue*2 do
            table.insert(emptyIndexList,i)
        end
    end
    local allocCount = #refreshMap*2
    while(allocCount > 0 ) do
        local cardIndex = math.random(1,#refreshMap)
        local cardMapIndex1 = self:InsetCardMap(cardIndex,refreshMap,emptyIndexList)
        local cardMapIndex2 = self:InsetCardMap(cardIndex,refreshMap,emptyIndexList)
        self.cardMap[cardMapIndex1].target = cardMapIndex2
        self.cardMap[cardMapIndex2].target = cardMapIndex1
        table.remove(refreshMap,cardIndex)
        allocCount = allocCount -2
    end
end

function class:InsetCardMap(cardIndex,refreshMap,emptyIndexList,isAll)
    local emptyIndex = math.random(1,#emptyIndexList)
    local cardMapIndex = emptyIndexList[emptyIndex]
    ---@class CardInfo
    local cardInfo ={}
    cardInfo.pic = refreshMap[cardIndex]
    cardInfo.target =0
    cardInfo.isFinsh = false
    cardInfo.findDir = Const.MiniGameLinkFindDir.None
    if isAll then
        cardInfo.line = 0
        cardInfo.row =0
    else
        cardInfo.line = self.cardMap[cardMapIndex].line
        cardInfo.row = self.cardMap[cardMapIndex].row
    end
    self.cardMap[cardMapIndex] = cardInfo
    table.remove(emptyIndexList,emptyIndex)
    return cardMapIndex
end

class.OnInitOther(class)
class.OnGameStartInit(class)
class.OnGameRealStart(class)

function table.findIndex(t, item)
    for k, v in pairs(t) do
        if v == item then
            return k
        end
    end
end

function table.contains(t, item)
    if not t then return false end
    return table.findIndex(t, item) and true or false
end

local allFind = table.sort(class.allInitPictrue)
local findList ={}
--[[for i = 1, #class.cardMap do
    if class.cardMap[i].pic ~= nil then
        if not table.contains(findList,class.cardMap[i].pic) then
            local canMove,pos = class:CheckCanLink(i,class.cardMap[i].target)
            table.insert(findList,class.cardMap[i].pic)
        end
    end
end]]--

local canMove,pos = class:CheckCanLink(12,42)

print("check finsh :"..tostring(canMove))

local Common ={}
function Common.NumberToChineseNumber(number)
    local chinese = {"零","一","二","三","四","五","六","七","八","九","十","百","千"}
    if number > 10000 then
        return number
    end
    if number <= 10 then
        return chinese[number+1]
    end
    local valueIndex = {}
    while(number > 0) do
       table.insert(valueIndex,math.floor(number % 10))
        number = math.floor(number / 10)
    end

    local multiple = 11 + #valueIndex -2
    local str = ""
    for i=#valueIndex , 1,-1 do
        if valueIndex[i] == 0 then
            if i ~= 1 and valueIndex[i+1] ~= 0 then
                local temp = i
                for j=i-1,1,-1 do
                    if valueIndex[j] ~= 0 then
                        temp = j
                        break
                    end
                end
                if temp == i then
                    break
                end
                i = temp
                str = str ..chinese[1]
            end
        else
            str = str .. chinese[valueIndex[i]+1] --加上第一位
            if i ~= 1 then
                str = str .. chinese[multiple]
            end
        end
        multiple = multiple -1
    end
    return str
end

print("1010\t"..Common.NumberToChineseNumber(1010))
print("999\t"..Common.NumberToChineseNumber(1001))
print("800\t"..Common.NumberToChineseNumber(3000))
print("99\t"..Common.NumberToChineseNumber(300))
print("10\t"..Common.NumberToChineseNumber(10))
print("11\t"..Common.NumberToChineseNumber(20))
print("9\t"..Common.NumberToChineseNumber(9))
print("0\t"..Common.NumberToChineseNumber(0))


local table = {
   a= "111111",b="222222222"
}

local table2 = setmetatable({typeName = "int"},table)
table2.a ="333333333333"

for k,v in pairs(table2) do
    print(k.."\t"..v)
end

local myTable = {}
local a =setmetatable(myTable,{isNew = true, __index=function(myTable,key)
    return rawget(getmetatable(myTable),key)
end})

print(tostring(a.isNew))

function string.split(str, reps)
    local resultStrList = {}
    string.gsub(str,'[^'..reps..']+',function ( w ) 
        resultStrList[#resultStrList + 1] = w 
    end)
    return resultStrList
end

local a = "aaaaaa得bbbbbb▄▃cccccccccccc"
local c = string.split(a,"&*(*&")


for i=1,#c do
    print(c[i])
end



function DogLiu(value)
    local value1 = value
    return function()
        return value1 +1
    end
end

local dog1 = DogLiu(1)
local dog2 = DogLiu(2)
print("dog1:"..dog1().."\tdog2:"..dog2())

Component = require("Component")
Util = require("Util")
TestComponent()