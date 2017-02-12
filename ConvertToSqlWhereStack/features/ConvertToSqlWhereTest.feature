Feature: ConvertToSqlWhereTest
	將 [bb:] action (cc[,cc]*) 轉成 sql where 語句
	1.action 的值是add or not equals 四个中逻辑操作符中的任意一个,随机出现
	2.[bb:] 方括号表示可省略内容，* 表示零个或者多个。bb对应数据结构定义中的name、age、pwd 等字段。
	3.	and, or 是布尔逻辑操作，可以接收两个的cc，分别代表“与” 和“或” 操作；
		not 是逻辑否操作，只接受一个cc,表示不等于；
		equals 只接收一个cc，用来选择指定bb 和所提供的cc 相等的所有数据条目，例如：
			equals("adam" ) // 返回所有任一个bb等于 "adam" 的数据
		name:equals("adam jones" ) // 返回所有name等于 "adam jones" 的数据
			age:equals(40) // 返回所有age等于40的数据

	需求：
		现在需要写个程序，对形如[bb:] action (cc[,cc]*)形式的字符串生成sql语句where后面的部分

	举例：
		and(or(name:equals(" admin" ), age:equals(32)), not(gender:equals(" male" )))
		的输入结果为：(name="admin" or age=32 ) and gender <> "male"

	註:
		未實作 equals("adam" )

Scenario Outline:轉換 
	Given 將輸入的語句 <input>
	When 進行轉換後
	Then 得到 sql where 語句 <output>
	Examples: 
	| example description                       | input                                                                         | output                                                 |
	| N.EQ.                                     | age:equals(20)                                                                | age = 20                                               |
	| S.EQ.                                     | age:equals("20")                                                              | age = '20'                                             |
	| N.NE.                                     | not(age:equals(20))                                                           | age != 20                                              |
	| S.NE.                                     | not(age:equals("20"))                                                         | age != '20'                                            |
	| N.EQ. and. N.EQ.                          | and(age:equals(20),salary:equals(22000))                                      | age = 20 and salary = 22000                            |
	| N.EQ. and. N.EQ. and. N.EQ.               | and(age:equals(20),salary:equals(22000),seniority:equals(2))                  | age = 20 and salary = 22000 and seniority = 2          |
	| N.EQ. and. N.NE. and. N.EQ.               | and(age:equals(20),not(salary:equals(22000)),seniority:equals(2))             | age = 20 and salary != 22000 and seniority = 2         |
	| S.EQ. and. N.NE. and. S.EQ.               | and(not(age:equals("20")),not(salary:equals(22000)),seniority:equals("2"))    | age != '20' and salary != 22000 and seniority = '2'    |
	| N.EQ. or. N.EQ.                           | or(age:equals(20),salary:equals(22000))                                       | age = 20 or salary = 22000                             |
	| N.NE. or. S.NE. or N.EQ.                  | or(not(age:equals(20)),not(salary:equals("22000")),seniority:equals(2))       | age != 20 or salary != '22000' or seniority = 2        |
	| (S.NE. and. N.EQ.) or N.EQ.               | or(and(not(age:equals("20")),salary:equals(22000)),seniority:equals(2))       | ( age != '20' and salary = 22000 ) or seniority = 2    |
	| N.EQ. and. (S.NE. or. S.NE.)              | and(age:equals(20),or(not(salary:equals("22000")),not(seniority:equals("2"))) | age = 20 and ( salary != '22000' or seniority != '2' ) |
	| (N.NE. and. S.EQ.) or. (S.NQ. and. N.EQ.) | or(and(not(b:equals(2)),c:equals("3")),and(not(d:equals("4")),a:equals(1)))   | ( b != 2 and c = '3' ) or ( d != '4' and a = 1 )       |
	| (N.NE. or. S.EQ.) and. (S.NQ. or. N.EQ.)  | and(or(not(b:equals(2)),c:equals("3")),or(not(d:equals("4")),a:equals(1)))    | ( b != 2 or c = '3' ) and ( d != '4' or a = 1 )        |
	# N:number S:string EQ: equals NE: not equals