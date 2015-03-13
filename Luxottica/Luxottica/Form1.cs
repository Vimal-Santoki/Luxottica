using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime;
using System.IO.Compression;
using Shell32;


namespace Luxottica
{
    public partial class Form1 : Form
    {

        string BrandToProcess = "";
        string FileStoragePath = "";
        string UPCCodeZipFile = "";
        string LuxotticaRawFile = "";
        int BrandID = 0;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]//, CharSet = CharSet.Unicode)
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);


        DataTable dtUPCCode = new DataTable();


        const int WM_GETTEXT = 0xD;//0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;
        const uint WM_SETTEXT = 0x000C;
        const uint SW_SHOW = 5;


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        int Steps = 0, Steps1 = 0, BrandSteps = 0, ProcessedBrand = 0;
        string[] strRecords = null, strBrands = null;
        DataTable dt = new DataTable();
        int Count = 0, tmpCount = 0, BrandCount = 0, FindWindowCountr = 0;
        string BrandName = "", BrandCode = "", ColorNo = "";
        string FileName = "", strUPCCodeFile = "";
        public Form1(string Brand)
        {
            InitializeComponent();
            BrandToProcess = Brand;
            dt.Columns.Add("ProductCode");
            dt.Columns.Add("BrandName");
            dt.Columns.Add("BrandCode");
            dt.Columns.Add("BrandCode2");
            dt.Columns.Add("ColorNo");
            dt.Columns.Add("ModelCode");
            dt.Columns.Add("ModelName");
            dt.Columns.Add("FrontColorFamily");
            dt.Columns.Add("LensesMaterial");
            dt.Columns.Add("lensesproperties");
            dt.Columns.Add("lensescolor");
            dt.Columns.Add("Price");
            dt.Columns.Add("Suggestedretailprice");
            dt.Columns.Add("CATEGORY");
            dt.Columns.Add("GENDER");
            dt.Columns.Add("SHAPE");
            dt.Columns.Add("FRONTMATERIAL");
            dt.Columns.Add("TEMPLE");
            dt.Columns.Add("BRIDGEDESIGN");
            dt.Columns.Add("GEOFIT");
            dt.Columns.Add("BASE");
            dt.Columns.Add("RX-ABLE");
            dt.Columns.Add("Availability");
            dt.Columns.Add("Size");
            dt.Columns.Add("Bridge");
            dt.Columns.Add("A");
            dt.Columns.Add("B");
            dt.Columns.Add("ED");
            dt.Columns.Add("TempleLength");
            dt.Columns.Add("UPCCode");
            dtUPCCode.Columns.Add("Model");
            dtUPCCode.Columns.Add("Size");
            dtUPCCode.Columns.Add("Color");
            dtUPCCode.Columns.Add("UPCCode");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtUser.Text.Trim() == "")
            {
                MessageBox.Show("Luxottica User Name should not be blank", "Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUser.Focus();
                return;
            }
            else if (txtPassword.Text.Trim() == "")
            {
                MessageBox.Show("Luxottica Password should not be blank", "Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtPassword.Focus();
                return;
            }
            webBrowser2.Visible = false;
            webBrowser4.Visible = false;
            webBrowser3.Visible = true;
            webBrowser3.Navigate("http://my.luxottica.com");
            if (System.IO.File.Exists("Settings.txt"))
                System.IO.File.Delete("Settings.txt");
            if (System.IO.File.Exists("Brands.txt"))
                System.IO.File.Delete("Brands.txt");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
                return;

        }

        public void StartParsingItems()
        {
            //if (Count <= 5)
            if (webBrowser2 != null)
                webBrowser2.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            WriteToExcel();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //if(Count<=20 && strRecords.Length>Count)
            if (strRecords.Length - 1 >= Count)
            {
                string[] Data = strRecords[Count].Split('#');
                Count++;
                BrandName = Data[0].Split('~')[1];
                BrandCode = Data[1].Split('~')[1];
                ColorNo = Data[2].Split('~')[1];
                InitWebBrowser2();
                File.AppendAllText("LogLuxoticca.txt", Data[3].Split('~')[1]+Environment.NewLine);
                webBrowser2.Navigate(Data[3].Split('~')[1]);
            }
            else
            {
                MessageBox.Show("Files Saved Successfully in @" + folderBrowserDialog1.SelectedPath, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        public void WriteToExcel()
        {
            Excel.Application oXL = null;
            Excel.Workbook oWB;
            Excel.Worksheet oSheet;
            Excel.Range oRange;
            oXL = new Excel.Application();
            string ProductCode = "";
            string Fn = (FileStoragePath == "" ? folderBrowserDialog1.SelectedPath : FileStoragePath) + "\\" + LuxotticaRawFile;
            if (File.Exists(Fn))
            {
                oWB = oXL.Workbooks.Open(Fn, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                oSheet = oWB.Sheets["Data"] as Excel.Worksheet;
                oRange = oSheet.UsedRange;
                int RowCount = oRange.Rows.Count;
                RowCount++;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ProductCode= dt.Rows[i]["BrandCode"].ToString() + "_" + dt.Rows[i]["ModelCode"].ToString() + "_" + dt.Rows[i]["Size"].ToString() + dt.Rows[i]["Bridge"].ToString();
                    dt.Rows[i]["ProductCode"] = ProductCode.Replace(" ", "").Replace("/", "_");
                    foreach (System.Data.DataColumn dCol in dt.Columns)
                    {
                        oRange = (Excel.Range)oSheet.Cells[RowCount, dt.Columns.IndexOf(dCol) + 1];
                        oRange.NumberFormat = "@";

                        oSheet.Cells[RowCount, dt.Columns.IndexOf(dCol) + 1] = dt.Rows[i][dCol];

                    }
                    RowCount++;
                }
                oRange = oSheet.get_Range(oSheet.Cells[1, 1],
                       oSheet.Cells[RowCount, dt.Columns.Count]);
                oRange.EntireColumn.AutoFit();

                // Save the sheet and close 
                oSheet = null;
                oRange = null;
                oWB.Save();
                oWB.Close(Missing.Value, Missing.Value, Missing.Value);
                oWB = null;
                oXL.Quit();
                dt.Rows.Clear();
            }
            else
            {

                int RowCount = 1;
                // Set some properties 
                oXL.Visible = false;
                oXL.DisplayAlerts = false;

                // Get a new workbook. 
                oWB = oXL.Workbooks.Add(Missing.Value);

                // Get the Active sheet 
                oSheet = (Excel.Worksheet)oWB.ActiveSheet;
                oSheet.Name = "Data";
                //oRange=oSheet.Columns["AB:AB"].EntireColumn;
                foreach (DataColumn dc in dt.Columns)
                {
                    oSheet.Cells[RowCount, dt.Columns.IndexOf(dc) + 1] = dc.ColumnName;
                }

                RowCount++;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ProductCode = dt.Rows[i]["BrandCode"].ToString() + "_" + dt.Rows[i]["ModelCode"].ToString() + "_" + dt.Rows[i]["Size"].ToString() + dt.Rows[i]["Bridge"].ToString();
                    dt.Rows[i]["ProductCode"] = ProductCode.Replace(" ", "").Replace("/", "_");
                    foreach (System.Data.DataColumn dCol in dt.Columns)
                    {
                        oRange = (Excel.Range)oSheet.Cells[RowCount, dt.Columns.IndexOf(dCol) + 1];
                        oRange.NumberFormat = "@";
                        oSheet.Cells[RowCount, dt.Columns.IndexOf(dCol) + 1] = dt.Rows[i][dCol];

                    }
                    RowCount++;
                }
                oRange = oSheet.get_Range(oSheet.Cells[1, 1],
                       oSheet.Cells[RowCount, dt.Columns.Count]);
                oRange.EntireColumn.AutoFit();

                // Save the sheet and close 
                oSheet = null;
                oRange = null;
                oWB.SaveAs(Fn, Excel.XlFileFormat.xlWorkbookDefault,
                    Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                    Excel.XlSaveAsAccessMode.xlExclusive,
                    Missing.Value, Missing.Value, Missing.Value,
                    Missing.Value, Missing.Value);
                oWB.Close(Missing.Value, Missing.Value, Missing.Value);
                oWB = null;
                oXL.Quit();
                dt.Rows.Clear();
            }
        }

        public void CreateTemplate(string TemplateFileName, DataTable dtTemplateData)
        {
            string Paths = FileStoragePath == "" ? folderBrowserDialog1.SelectedPath : FileStoragePath;
            //MessageBox.Show(Paths + "\\" + TemplateFileName);
            if (File.Exists(Paths + "\\" + TemplateFileName))
                File.Move(Paths + "\\" + TemplateFileName, Paths + "\\Old_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + "_" + TemplateFileName);
            foreach (DataColumn dc in dtTemplateData.Columns)
            {
                File.AppendAllText(Paths + "\\" + TemplateFileName, dc.ColumnName + "\t");
            }
            File.AppendAllText(Paths + "\\" + TemplateFileName, Environment.NewLine);
            for (int i = 0; i < dtTemplateData.Rows.Count; i++)
            {
                foreach (System.Data.DataColumn dCol in dtTemplateData.Columns)
                {
                    File.AppendAllText(Paths + "\\" + TemplateFileName, dtTemplateData.Rows[i][dCol].ToString() + "\t");
                }
                File.AppendAllText(Paths + "\\" + TemplateFileName, Environment.NewLine);
            }
        }

        protected void DelayExecution(int nSeconds, bool IsScrollDown)
        {
            System.DateTime tmCurrent;
            System.DateTime tmStart = System.DateTime.Now;

            System.TimeSpan tmspStart = new TimeSpan(tmStart.Hour,
            tmStart.Minute, tmStart.Second);

            double dStartSeconds = tmspStart.TotalSeconds;
            double dCurrentSeconds = dStartSeconds;

            while (dCurrentSeconds - dStartSeconds < nSeconds)
            {
                if (IsScrollDown)
                {
                    webBrowser3.Document.Window.ScrollTo(0, webBrowser3.Height + webBrowser3.Document.GetElementsByTagName("HTML")[0].ScrollTop);
                }
                Application.DoEvents();
                tmCurrent = System.DateTime.Now;
                System.TimeSpan tmspCurrent = new TimeSpan(tmCurrent.Hour,
                tmCurrent.Minute, tmCurrent.Second);

                dCurrentSeconds = tmspCurrent.TotalSeconds;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
                return;
            DelayExecution(2, false);
            if (webBrowser2.Document == null)
                DelayExecution(2, false);
            HtmlElementCollection trs = webBrowser2.Document.GetElementById("brand-products-info").GetElementsByTagName("tr");
            string BrandCodeOriginal = "";
            HtmlElementCollection currentbrandslis = webBrowser2.Document.GetElementById("breadcrumbs").GetElementsByTagName("li");
            foreach (HtmlElement currentbrandsli in currentbrandslis)
            {
                if (currentbrandsli.GetElementsByTagName("span").Count > 0 && currentbrandsli.GetElementsByTagName("span")[0].GetAttribute("classname").ToLower() == "current")
                {
                    BrandCodeOriginal = currentbrandsli.GetElementsByTagName("span")[0].InnerText.Replace(">", "").Trim();
                }
            }
            foreach (HtmlElement tr in trs)
            {
                HtmlElement div = tr.GetElementsByTagName("td")[1].GetElementsByTagName("div")[0];
                DataRow drow = dt.NewRow();
                
                drow["BrandCode2"] = BrandCode;
                drow["BrandCode"] = BrandCodeOriginal;
                drow["BrandName"] = BrandName;
                drow["ColorNo"] = ColorNo;
                drow["ModelCode"] = div.GetElementsByTagName("h1")[0].InnerText;
                drow["ModelName"] = div.GetElementsByTagName("h1")[1].InnerText;
                
                HtmlElementCollection spans = div.GetElementsByTagName("span");
                foreach (HtmlElement span in spans)
                {
                    if (span.InnerText != null && span.InnerText.Contains(":") && span.InnerText.Split(':').Length == 2 && dt.Columns.Contains(span.InnerText.Split(':')[0].Trim().Replace(" ", "")))
                        drow[span.InnerText.Split(':')[0].Trim().Replace(" ", "")] = span.InnerText.Split(':')[1].Trim();
                    //if (span.InnerText.ToLower().Contains("suggested"))
                    //{
                    //    drow["Suggestedretailprice"] = span.FirstChild.InnerText;
                    //}
                }
                //drow["lensesmaterial"] = div.GetElementsByTagName("span")[3].InnerText;
                //drow["lensesproperties"] = div.GetElementsByTagName("span")[5].InnerText;
                //drow["lensescolor"] = div.GetElementsByTagName("span")[7].InnerText;
                HtmlElementCollection spans2 = tr.GetElementsByTagName("td")[1].GetElementsByTagName("span");
                foreach (HtmlElement spans1 in spans2)
                {
                    if (spans1.GetAttribute("classname").ToLower() == "price")
                    {
                        drow["Price"] = spans1.InnerText.Replace("$", "").Trim();
                    }
                    else if (spans1.GetAttribute("classname").ToLower() == "price-info")
                    {
                        drow["Suggestedretailprice"] = spans1.Children[0].InnerText;
                    }
                }

                HtmlElementCollection firstlables = tr.GetElementsByTagName("td")[2].GetElementsByTagName("li")[0].GetElementsByTagName("label");
                if (firstlables.Count > 0)
                    drow["Size"] = int.Parse(firstlables[0].InnerText.ToLower().Replace("size", "").Trim());
                HtmlElementCollection fistAvail = tr.GetElementsByTagName("td")[2].GetElementsByTagName("li")[0].GetElementsByTagName("span");
                if (fistAvail.Count > 0)
                    drow["Availability"] = fistAvail[0].InnerText;
                FillInfo(drow);
                DataRow[] drows = dtUPCCode.Select("Model='" + drow["BrandCode"].ToString().Trim() + "' AND Size='" + drow["Size"].ToString().ToLower().Replace("size", "").Trim() + "' AND Color='" + drow["ModelCode"].ToString().Trim() + "'");
                if (drows.Length > 0)
                {
                    drow["UPCCode"] = drows[0]["UpcCode"].ToString();
                }
                dt.Rows.Add(drow);
                if (tr.GetElementsByTagName("td")[2].GetElementsByTagName("li").Count > 1)
                {
                    DataRow drow1;
                    drow1 = dt.NewRow();
                    drow1.ItemArray = drow.ItemArray;
                    HtmlElementCollection secondSize = tr.GetElementsByTagName("td")[2].GetElementsByTagName("li")[1].GetElementsByTagName("label");
                    if (secondSize.Count > 0)
                        drow1["Size"] = int.Parse(secondSize[0].InnerText.ToLower().Replace("size", "").Trim());
                    HtmlElementCollection SecondAvail = tr.GetElementsByTagName("td")[2].GetElementsByTagName("li")[1].GetElementsByTagName("span");
                    if (SecondAvail.Count > 0)
                        drow1["Availability"] = SecondAvail[0].InnerText;
                    drows = dtUPCCode.Select("Model='" + drow1["BrandCode"].ToString().Trim() + "' AND Size='" + drow1["Size"].ToString().ToLower().Replace("size", "").Trim() + "' AND Color='" + drow1["ModelCode"].ToString().Trim() + "'");
                    FillInfo(drow1);
                    if (drows.Length > 0)
                    {
                        drow1["UPCCode"] = drows[0]["UpcCode"].ToString();
                    }

                    dt.Rows.Add(drow1);
                    if (tr.GetElementsByTagName("td")[2].GetElementsByTagName("li").Count == 3)
                    {
                        DataRow drow2;
                        drow2 = dt.NewRow();
                        drow2.ItemArray = drow.ItemArray;
                        HtmlElementCollection ThirdSize = tr.GetElementsByTagName("td")[2].GetElementsByTagName("li")[2].GetElementsByTagName("label");
                        if (ThirdSize.Count > 0)
                            drow2["Size"] = int.Parse(ThirdSize[0].InnerText.ToLower().Replace("size", "").Trim());
                        HtmlElementCollection ThirdAvail = tr.GetElementsByTagName("td")[2].GetElementsByTagName("li")[2].GetElementsByTagName("span");
                        if (ThirdAvail.Count > 0)
                            drow2["Availability"] = ThirdAvail[0].InnerText;
                        FillInfo(drow2);
                        drows = dtUPCCode.Select("Model='" + drow2["BrandCode"].ToString().Trim() + "' AND Size='" + drow2["Size"].ToString().ToLower().Replace("size", "").Trim() + "' AND Color='" + drow2["ModelCode"].ToString().Trim() + "'");
                        if (drows.Length > 0)
                        {
                            drow2["UPCCode"] = drows[0]["UpcCode"].ToString();
                        }
                        dt.Rows.Add(drow2);
                    }
                }

                //dt.Rows.Add(drow);

            }
            trs = null;
            StartParsingItems();
        }

        public void InitWebBrowser2()
        {
            webBrowser2 = new WebBrowser();
            this.groupBox2.Controls.Add(webBrowser2);
            this.webBrowser2.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser2.Location = new System.Drawing.Point(617, 79);
            webBrowser2.MinimumSize = new System.Drawing.Size(20, 20);
            webBrowser2.Name = "webBrowser2";

            webBrowser2.ScriptErrorsSuppressed = true;
            webBrowser2.Size = new System.Drawing.Size(547, 430);
            webBrowser2.TabIndex = 1;
            
            //this.Controls.Add(webBrowser2);
            webBrowser2.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser2_DocumentCompleted);
            Application.DoEvents();
        }

        public void FillInfo(DataRow drow)
        {
            HtmlElementCollection divs = webBrowser2.Document.GetElementById("moreInfoProduct").GetElementsByTagName("div");
            foreach (HtmlElement div in divs)
            {
                if (div.GetAttribute("classname").ToLower() == "col-left")
                {
                    HtmlElementCollection ps = div.GetElementsByTagName("p");
                    foreach (HtmlElement p in ps)
                    {
                        string[] Data = p.InnerText.Split(':');
                        if (Data.Length == 2)
                        {
                            if (drow.Table.Columns.Contains(Data[0].Replace(" ", "")))
                            {
                                drow[Data[0].Replace(" ", "")] = Data[1];
                            }
                        }
                    }
                }
                else if (div.GetAttribute("classname").ToLower() == "col-right")
                {
                    HtmlElementCollection trs = div.GetElementsByTagName("table")[0].GetElementsByTagName("tbody")[0].GetElementsByTagName("tr");
                    foreach (HtmlElement tr in trs)
                    {
                        if (tr.GetElementsByTagName("td")[0].InnerText.Trim() == drow["size"].ToString().ToLower().Replace("size", "").Trim())
                        {
                            HtmlElementCollection tds = tr.GetElementsByTagName("td");
                            drow["bridge"] = Convert.ToInt16(Convert.ToDouble(tds[1].InnerText));
                            drow["A"] = "";//tds[2].InnerText;
                            drow["B"] = tds[2].InnerText;
                            drow["ED"] = tds[3].InnerText;
                            drow["TempleLength"] = tds[4].InnerText;
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void webBrowser3_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
                return;
            if (BrandSteps == 0)
            {
                if (webBrowser3.Document.GetElementById("WC_AccountDisplay_FormInput_logonId_In_Logon_1") != null)
                {
                    webBrowser3.Document.GetElementById("WC_AccountDisplay_FormInput_logonId_In_Logon_1").InnerText = txtUser.Text.Trim();
                    webBrowser3.Document.GetElementById("WC_AccountDisplay_FormInput_logonPassword_In_Logon_1").InnerText = txtPassword.Text.Trim();
                    webBrowser3.Document.GetElementById("WC_AccountDisplay_links_2").InvokeMember("click");
                    BrandSteps = 1;

                    DelayExecution(5, false);
                }

            }
            if (BrandSteps == 1)
            {
                HtmlElementCollection divs = webBrowser3.Document.GetElementById("page-container").GetElementsByTagName("div");
                webBrowser3.Navigate("https://my.luxottica.com/webapp/wcs/stores/servlet/AdvancedSearchView?storeId=10001&urlRequestType=Base&langId=-1&catalogId=10001");
                DelayExecution(5, false);
                BrandSteps = 2;
            }
            if (BrandSteps == 2)
            {
                HtmlElementCollection sections = webBrowser3.Document.GetElementsByTagName("section");
                foreach (HtmlElement section in sections)
                {
                    //MessageBox.Show(div.GetAttribute("classname").ToLower());
                    if (section.GetAttribute("classname").ToLower() == "advanced-search")
                    {
                        HtmlElementCollection lis = section.GetElementsByTagName("form")[0].GetElementsByTagName("section")[0].GetElementsByTagName("li");
                        foreach (HtmlElement li in lis)
                        {
                            chkBrandList.Items.Add(li.GetElementsByTagName("a")[0].InnerText);
                        }
                    }
                }
                Application.DoEvents();
                if (chkBrandList.Items.Count > 0)
                {
                    chkAllBrands.Enabled = true;
                    BrandSteps = 3;
                    if (BrandID > 0)
                    {
                        btnScrapper_Click(null, null);
                    }
                }
            }
            if (BrandSteps == 4)
            {
                int tempCountStop = 0;
                DelayExecution(5, true);
                HtmlElementCollection lis = webBrowser3.Document.GetElementsByTagName("li");
                foreach (HtmlElement li in lis)
                {
                    if (li.GetAttribute("classname").ToUpper().Trim().Contains("ITEM"))
                    {
                        string link = li.GetElementsByTagName("a")[0].GetAttribute("href");
                        string BrandName = li.GetElementsByTagName("a")[0].GetElementsByTagName("h1")[0].InnerText;
                        string BrandCode = li.GetElementsByTagName("a")[0].GetElementsByTagName("h3")[0].InnerText;
                        string ColorNo = li.GetElementsByTagName("a")[0].GetElementsByTagName("hgroup")[0].GetElementsByTagName("span")[0].InnerText;
                        System.IO.File.AppendAllText("Settings.txt", "BrandName~" + BrandName + "#BrandCode~" + BrandCode + "#ColorNo~" + ColorNo + "#URL~" + link + Environment.NewLine);
                        BrandSteps = 5;
                        tempCountStop++;
                    }
                }
                //if (tmpCount <= 0)
                {
                    bool Eof = true;
                    HtmlElementCollection navs = webBrowser3.Document.GetElementById("Search_Result_div").GetElementsByTagName("nav");
                    foreach (HtmlElement nav in navs)
                    {
                        if (nav.GetAttribute("classname").ToLower() == "pagination")
                        {
                            HtmlElementCollection aas = nav.GetElementsByTagName("a");
                            foreach (HtmlElement a in aas)
                            {
                                if (a.GetAttribute("classname").ToLower() == "next-page")
                                {
                                    a.InvokeMember("click");
                                    BrandSteps = 4;
                                    tmpCount++;
                                    Eof = false;
                                    break;
                                }
                            }
                            if (BrandSteps == 4)
                                break;
                        }
                    }
                    if (Eof)
                    {
                        strRecords = System.IO.File.ReadAllLines("Settings.txt");
                        if (strRecords.Length > 0)
                        {
                            timer1.Start();
                            timer1.Enabled = true;
                            FindWindowCountr = 3;
                        }
                    }
                }
            }
        }
        private void webBrowser4_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlElementCollection ass = webBrowser4.Document.GetElementById("col-dx").GetElementsByTagName("a");
            foreach (HtmlElement a in ass)
            {
                if (a.GetAttribute("classname").ToLower() == "select-all")
                {
                    a.InvokeMember("click");
                    System.Threading.Thread.Sleep(1000);
                    webBrowser4.Document.GetElementById("downloadSubmit").InvokeMember("click");
                    timer1.Enabled = true;
                    timer1.Start();
                }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (FindWindowCountr == 0)
            {
                this.TopMost = true;
                this.TopMost = false;
                IntPtr hwnd = FindWindow(null, "File Download");
                IntPtr nullptr = (IntPtr)0;
                IntPtr hokBtn = FindWindowEx(hwnd, nullptr, "Button", "&Save");
                if (hwnd != nullptr)
                {
                    timer1.Stop();
                    timer1.Enabled = false;
                    BringWindowToTop(hwnd);
                    ShowWindow(hwnd, SW_SHOW);
                    SetActiveWindow(hwnd);
                    DelayExecution(2, false);
                    IntPtr res = SendMessage(hokBtn, (int)0x00F5, 0, IntPtr.Zero);
                    FindWindowCountr = 1;
                    timer1.Start();
                    timer1.Enabled = true;
                }
            }
            if (FindWindowCountr == 1)
            {
                this.TopMost = true;
                this.TopMost = false;
                IntPtr hwnd = FindWindow(null, "Save As");
                BringWindowToTop(hwnd);
                ShowWindow(hwnd, SW_SHOW);
                SetActiveWindow(hwnd);
                IntPtr nullptr = (IntPtr)0;
                if (hwnd != nullptr)
                {
                    timer1.Stop();
                    timer1.Enabled = false;
                    FindWindowCountr = 2;
                    string str = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
                    UPCCodeZipFile = "LuxotticaUPC" + DateTime.Now.ToString("ddMMyyyyhhmmss");
                    SendKeys.Send("%N");
                    SendKeys.SendWait(str + UPCCodeZipFile + ".zip");
                    SendKeys.Send("%S");
                    FindWindowCountr = 2;
                    timer1.Start();
                    timer1.Enabled = true;
                }
            }
            if (FindWindowCountr == 2)
            {
                IntPtr hwnd = FindWindow(null, "Download complete");
                BringWindowToTop(hwnd);
                ShowWindow(hwnd, SW_SHOW); SetActiveWindow(hwnd);
                IntPtr nullptr = (IntPtr)0;
                IntPtr hokBtn = FindWindowEx(hwnd, nullptr, "Button", "Close");
                //IntPtr res = SendMessage(hokBtn, (int)0x00F5, 0, IntPtr.Zero);
                timer1.Stop();
                timer1.Enabled = false;
                DelayExecution(35, false);
                //if (hokBtn == nullptr)
                {
                    timer1.Stop();
                    timer1.Enabled = false;
                    FindWindowCountr = 4;
                    Decompress();
                    FillUpcCodes();
                }
            }
            if (FindWindowCountr == 3)
            {
                timer1.Stop();
                timer1.Enabled = false;
                FindWindowCountr = 6;
                webBrowser3.Visible = false;
                webBrowser2.Visible = true;
                StartParsingItems();
            }
        }
        public void Decompress()
        {
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Articoli.dat"))
                File.Move(System.AppDomain.CurrentDomain.BaseDirectory + "Articoli.dat", System.AppDomain.CurrentDomain.BaseDirectory + "Articoli" + DateTime.Now.ToString("ddMMyyhhmmss") + ".dat");

            Shell sc = new Shell();
            //Directory.CreateDirectory("D:\\dataluxo") ;
            Folder output = sc.NameSpace(System.AppDomain.CurrentDomain.BaseDirectory);
            Folder input = sc.NameSpace(System.AppDomain.CurrentDomain.BaseDirectory + UPCCodeZipFile + ".zip");
            output.CopyHere(input.Items(), 4);
            DelayExecution(5, false);
        }
        public void FillUpcCodes()
        {
            string[] strUpcs = File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "Articoli.dat");
            foreach (string strcode in strUpcs)
            {
                DataRow drow = dtUPCCode.NewRow();
                drow["Model"] = strcode.Substring(0, 9).Trim();
                drow["Size"] = strcode.Substring(9, 4).Trim();
                drow["Color"] = strcode.Substring(13, 6).Trim();
                drow["UpcCode"] = strcode.Substring(19, 13).Trim();
                dtUPCCode.Rows.Add(drow);
            }
            webBrowser4.Visible = false;
            webBrowser3.Visible = true;
            HtmlElementCollection sections = webBrowser3.Document.GetElementsByTagName("section");
            foreach (HtmlElement section in sections)
            {
                //MessageBox.Show(div.GetAttribute("classname").ToLower());
                if (section.GetAttribute("classname").ToLower() == "advanced-search")
                {
                    HtmlElementCollection divs = section.GetElementsByTagName("form")[0].GetElementsByTagName("div");
                    foreach (HtmlElement div in divs)
                    {
                        if (div.GetAttribute("classname").ToLower() == "view-results")
                        {
                            BrandSteps = 4;
                            div.GetElementsByTagName("a")[0].InvokeMember("click");
                        }

                    }
                }
            }
        }

        private void btnScrapper_Click(object sender, EventArgs e)
        {
            // if (cmbBrands.SelectedIndex >= 0)
            {
                LuxotticaRawFile = "Brands_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx";
                bool Result = false;
                if (FileStoragePath == "")
                {
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        Result = true;
                    }
                }
                else
                    Result = true;
                if (Result)
                {
                    HtmlElementCollection sections = webBrowser3.Document.GetElementsByTagName("section");
                    foreach (HtmlElement section in sections)
                    {
                        //MessageBox.Show(div.GetAttribute("classname").ToLower());
                        if (section.GetAttribute("classname").ToLower() == "advanced-search")
                        {
                            HtmlElementCollection lis = section.GetElementsByTagName("form")[0].GetElementsByTagName("section")[0].GetElementsByTagName("li");
                            foreach (HtmlElement li in lis)
                            {
                                if (li.GetElementsByTagName("a")[0].GetAttribute("classname").ToLower() == "selected")
                                {
                                    li.GetElementsByTagName("a")[0].InvokeMember("click");
                                }
                            }
                        }
                    }
                    for (int i = 0; i < chkBrandList.CheckedItems.Count; i++)
                    {
                        foreach (HtmlElement section in sections)
                        {
                            //MessageBox.Show(div.GetAttribute("classname").ToLower());
                            if (section.GetAttribute("classname").ToLower() == "advanced-search")
                            {
                                HtmlElementCollection lis = section.GetElementsByTagName("form")[0].GetElementsByTagName("section")[0].GetElementsByTagName("li");
                                foreach (HtmlElement li in lis)
                                {
                                    if (li.GetElementsByTagName("a")[0].InnerText.ToLower() == chkBrandList.CheckedItems[i].ToString().ToLower() && li.GetElementsByTagName("a")[0].GetAttribute("classname").ToLower() != "selected")
                                    {
                                        li.GetElementsByTagName("a")[0].InvokeMember("click");
                                    }
                                }
                            }
                        }
                    }
                    webBrowser3.Visible = false;
                    webBrowser4.Visible = true;
                    webBrowser4.Navigate("https://my.luxottica.com/webapp/wcs/stores/servlet/LogonForm?catalogId=10001&langId=-1&storeId=10001&page=downloadDAT");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 800;
            this.Height = 435;
            //chkAllBrands.Enabled = false;
            //MessageBox.Show(BrandToProcess);
        }

        private void chkAllBrands_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < chkBrandList.Items.Count; i++)
            {
                chkBrandList.SetItemChecked(i, chkAllBrands.Checked);
                //((CheckBox)chkBrandList.Items[i]).Checked = chkAllBrands.Checked;
            }
          
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.Height >= 100)
            {
                groupBox1.Height = this.Height - 45;
                groupBox2.Height = this.Height - 45;
            }
            if (this.Width >= 150)
                groupBox2.Width = this.Width - groupBox1.Width - 30;
        }

        private void chkAllBrands_Click(object sender, EventArgs e)
        {
            if (chkBrandList.Items.Count <= 0)
                chkAllBrands.Checked = false;
                //return;

        }

    }
}
