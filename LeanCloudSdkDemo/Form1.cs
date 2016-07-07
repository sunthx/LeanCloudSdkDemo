using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AVOSCloud;

namespace LeanCloudSdkDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string _objectId = string.Empty;
        private string _queryClassName = "persons";

        #region Private

        private void Message(string value, bool add = false)
        {
            if (!add)
                this.txtInfo.Text = value;
            else
                this.txtInfo.Text += value + "\r\n";
        }

        private void Clear()
        {
            this.txtInfo.Text = string.Empty;
        }

        private async Task<string> AddTestDataItem(string className = "person")
        {
            string[] strings = new[] {"Tom", "Jim", "John", "Michelle", "Amy", "Mary", "Kim"};
            int[] ints = new[] {20, 12, 22, 23, 25, 26, 50, 65, 35, 40, 10, 100};

            string objectId = string.Empty;
            AVObject person = new AVObject(className)
            {
                ["name"] = strings[new Random().Next(0, 5)],
                ["age"] = ints[new Random().Next(0, 11)]
            };

            await person.SaveAsync().ContinueWith(t => { objectId = person.ObjectId; });

            return objectId;
        }

        private async Task AddTestDataItems(int count = 10, string className = "person")
        {
            for (int i = 0; i < count; i++)
            {
                await AddTestDataItem(className);
            }
        }

        private async Task AddTestData()
        {
            await AddTestDataItems(50, _queryClassName);
        }

        #endregion

        #region Add Delete Update

        private async void button1_Click(object sender, EventArgs e)
        {
            AVClient.Initialize(txtAppId.Text, txtAppKey.Text);
//            await AddTestData();
            Message("Initialize Ok");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            _objectId = await AddTestDataItem();
            Message("[Insert] 成功添加一条数据。[objectId] =" + _objectId);
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            _objectId = await AddTestDataItem();
            AVObject sunth = AVObject.CreateWithoutData("person", _objectId);
            await sunth.FetchAsync();

            Message($"[Get] 获取一条数据成功,[objectId] = {_objectId} && [name] = {sunth["name"]}");
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            //先添加一条数据，然后删除。
            _objectId = await AddTestDataItem();

            AVObject sunth = AVObject.CreateWithoutData("person", _objectId);
            await sunth.DeleteAsync();

            Message("[Delete] 删除一条数据成功 , [ObjectId] = " + _objectId);
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            //先添加一条数据，然后删除AVObject中的一个属性，这里的删除指的是置空。
            _objectId = await AddTestDataItem();

            AVObject sunth = AVObject.CreateWithoutData("person", _objectId);
            sunth.Remove("age");
            await sunth.SaveAsync();
            Message("[Delete] 删除 [Person] 中 [age] 属性成功, [ObjectId] = " + _objectId);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            //先添加一条数据，然后修改。
            _objectId = await AddTestDataItem();

            AVObject sunth = AVObject.CreateWithoutData("person", _objectId);
            sunth["name"] = "new sunth";
            sunth["age"] = 26;
            await sunth.SaveAsync();

            Message("[Update] 更新数据成功, [ObjectId] = " + _objectId);
        }

        #endregion

        #region Query

        private async void button5_Click(object sender, EventArgs e)
        {
            Clear();

            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.FindAsync();
            if (!result.Any())
            {
                Message("[Query] No Data !");
                return;
            }

            result.ToList().ForEach((item) => { Message($"[Query] ObjectId = {item.ObjectId} , Name = {item["name"]} , Age = {item["age"]}", true); });
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.CountAsync();
            Message($"[Query] Items Count = {result}");
        }

        private async void button9_Click(object sender, EventArgs e)
        {
            Clear();

            Message($"[Query] 操作符 >= ", true);
            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.WhereGreaterThanOrEqualTo("age", 26).FindAsync();
            Message($"[Query] WhereGreaterThanOrEqualTo('age', 26) 满足条件的个数 : {result.Count()}", true);

            Message($"[Query] 操作符 <= ", true);
            query = new AVQuery<AVObject>(_queryClassName);
            result = await query.WhereLessThanOrEqualTo("age", 26).FindAsync();
            Message($"[Query] WhereLessThanOrEqualTo('age', 26) 满足条件的个数 : {result.Count()}", true);
        }

        //Skip
        private async void button12_Click(object sender, EventArgs e)
        {
            var query = new AVQuery<AVObject>(_queryClassName);

            //使用Skip函数可以实现分页功能,对方法CountAsync无效果。
            Message($"[Skip] Items Count ： {await query.CountAsync()}");

            var result = await query.Skip(10).FindAsync();

            Message($"[Skip] 调用 query.Skip(10) 后结果集的 Count ： {result.Count()}");
        }

        //Limit
        private async void button11_Click(object sender, EventArgs e)
        {
            //限制查询的数量
            var result = await new AVQuery<AVObject>(_queryClassName).Limit(10).FindAsync();
            Message($"[Skip] 调用 query.Limit(10) 后结果集的 Count ： {result.Count()}");
        }

        private async void button13_Click(object sender, EventArgs e)
        {
            Clear();

            Message($"[Query] 操作符 == ", true);
            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.WhereEqualTo("age", 26).FindAsync();
            Message($"[Query] WhereEqualTo('age', 26) 满足条件的个数 : {result.Count()}", true);

            Message($"[Query] 操作符 != ", true);
            query = new AVQuery<AVObject>(_queryClassName);
            result = await query.WhereNotEqualTo("age", 26).FindAsync();
            Message($"[Query] WhereNotEqualTo('age', 26) 满足条件的个数 : {result.Count()}", true);
        }

        private async void button17_Click(object sender, EventArgs e)
        {
            Clear();

            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.WhereEndsWith("name", "m").FindAsync();
            Message($"[Query] WhereStartsWith('name', 'm') 满足条件的个数 : {result.Count()}", true);
        }

        private async void button16_Click(object sender, EventArgs e)
        {
            Clear();

            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.WhereStartsWith("name", "J").FindAsync();
            Message($"[Query] WhereStartsWith('name', 'J') 满足条件的个数 : {result.Count()}", true);
        }

        private async void button14_Click(object sender, EventArgs e)
        {
            Clear();

            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.WhereContains("name", "J").FindAsync();
            Message($"[Query] WhereContains('name', 'J') 满足条件的个数 : {result.Count()}", true);
        }

        private async void button10_Click(object sender, EventArgs e)
        {
            Clear();

            string[] conditions = {"Tom", "Jim"};
            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.WhereContainedIn("name", conditions).FindAsync();

            Message($"[Query] WhereContainedIn('name', 'J') 满足条件的个数 : {result.Count()}", true);
        }

        private async void button15_Click(object sender, EventArgs e)
        {
            Clear();

            string[] conditions = {"Tom", "Jim"};

            AVQuery<AVObject> query = new AVQuery<AVObject>(_queryClassName);
            var result = await query.WhereNotContainedIn("name", conditions).FindAsync();
            Message($"[Query] WhereNotContainedIn('name', 'J') 满足条件的个数 : {result.Count()}", true);
        }

        #endregion

        #region File

        private async void button18_Click(object sender, EventArgs e)
        {
            string fileName = "demo.png";

            //  通过文件流获得AVFile对象
            //            byte[] data;
            //
            //            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            //            {
            //                using (BinaryReader br = new BinaryReader(fs))
            //                    data = br.ReadBytes((int) fs.Length);
            //            }
            //
            //            AVFile localFile = new AVFile("demo.png",data);

            AVFile localFile = AVFile.CreateFileWithLocalPath(fileName, fileName);
            await localFile.SaveAsync();
            Message($"[Upload File] File ObjectId = {localFile.ObjectId}");
        }

        private async void button19_Click(object sender, EventArgs e)
        {
            string objectId = "577e09a96be3ff006a245790";
            await AVFile.GetFileWithObjectIdAsync(objectId).ContinueWith(t =>
            {
                var file = t.Result;
                file.DownloadAsync().ContinueWith(s =>
                {
                    var dataByte = file.DataByte;
                    using (BinaryWriter bw = new BinaryWriter(File.Open("demo2.png", FileMode.OpenOrCreate)))
                    {
                        bw.Write(dataByte);
                        bw.Close();
                    }
                });
            });
        }

        #endregion
    }
}