# Excel2Sqlite

在Excel中生成出数据库文件和脚本文件,工作人员只需要像平常一样操作Excel,不需要点击任何按钮.

Features
===
- 非常简单的使用
- 不需要任何代码
- 完全自动化
- 即时的反馈错误
- 智能提示,当输入函数时显示出相关的HelpDoc
- 光的速度(即使在表格很庞大的情况下)

Environment
===
- 请使用32位的Excel软件
- 使用VS2015或2017进行编译

Get Started
===
- Clone Git 仓库
- 手动添加克隆项目下的ScriptGenerate.dll引用
- 编译脚本
- 打开生成目录下的TestExcel_Auto.xlsx或者新建一个Excel文件
- 按下快捷键Alt+t,i打开面板选择浏览加入生成目录下的两个.xll文件
- 开始愉快的使用吧

How To Use
===
- 第一行填写策划的备注信息
- 第二行填写脚本变量名称
- 第三行填写脚本类型(目前仅支持操作4种类型 int,float,string,bool,int[],float[],string[],bool)
- 之后开始填写数据

如何在Excel中使用自定义类型
===
在第三行脚本类型中你可以这样定义**Id[int];Name[string]**
![index](https://github.com/pk27602017/Excel2Sqlite/raw/master/Image/自定义类型.png)
<br />
<br />
如果你希望定义自定义类型数组你可以使用**{Id[int];Name[string}**来进行定义
<br />
![index](https://github.com/pk27602017/Excel2Sqlite/raw/master/Image/自定义类型数组.png)

基础配置
===
在克隆下来的项目里包含了一个Config.txt文件,你可以在里面修改相关的参数达到修改导出内容的目的

代码生成
===
在克隆下来的项目里包含了一个GenerateTemplate.txt文件,你可以添加内容、删除里面的方法、修改命名空间,除此之外请不要修改里面的任何东西,如果你想引入自己的框架脚本,目前还暂不支持
