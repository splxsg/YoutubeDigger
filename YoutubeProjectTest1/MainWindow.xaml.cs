using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Win32;



using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeProjectTest1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        public class Informationdata
        {
            public string searched { set; get; }
            public string category { set; get; }
            public string achieved { set; get; }

            public Informationdata()
            { }

            public Informationdata infodata(string s1, string s2, string s3)
            {
                Informationdata indata = new Informationdata();
                indata.searched = s1;
                indata.category = s2;
                indata.achieved = s3;
                return indata;
            }
        }


        // private delegate void SearchDataFromServerDelegate(string s, string nextpagetoken);
        private delegate void FiveArguDelegate(string argu1, string argu2, string argu3, string argu4, int argu5);
        private delegate void UpdateProgress(Informationdata indata);
        private delegate void UpdateViewDelegate();
        private delegate void NoArguDelegate();
        private bool resultlistboxclickable;
        private bool newsearch = true;
        private string searchkeyword;
        //private string nextpgtoken = null;
        private YoutubeSearchResultContainer sResultContainer;
        //private string country = "";
        private bool stopsearch = false;
        private int searchamountperrequest = 20;
    


        public MainWindow()
        {
            InitializeComponent();
            loadmorebtn.IsEnabled = false;
            resultlistboxclickable = false;
            RealtimeinfoVB.Visibility = System.Windows.Visibility.Hidden;
            searchamountTB.Text = "50";
            repeatTolerateCB.SelectedIndex = 3;
            
            //  resultList.Width = this.Width * 0.6;
            // resultList.Height = (this.Height - 150) * 0.8;


        }

        private int getRepeatTolerateDays()
        {
            switch (repeatTolerateCB.SelectedIndex)
            {
                case 0: return 0;                    
                case 1: return 3;
                case 2: return 7;
                case 3: return 14;
                case 4: return 30;
                case 5: return 60;         
            }
            return -1;



        }

        private void button_Click(object sender, RoutedEventArgs ex)
        {
           
            if (searchbtn.Content.Equals("Search"))
            {
                if (searchamountTB.Text.ToString() == "")
                    MessageBox.Show("Please type in search volumn in amount box.");
                else if (int.Parse(searchamountTB.Text.ToString()) < 1 && int.Parse(searchamountTB.Text.ToString()) > 100)
                    MessageBox.Show("Please type in vaild search volumn in amount box. (1<= result <= 100)");
                else
                {
                    string country = countryCB.SelectionBoxItem.ToString();
                    string searchamount = searchamountTB.Text.ToString();
                    freezebtn();
                    searchbtn.Content = "Stop";
                    newsearch = true;
                    RealtimeinfoVB.Visibility = System.Windows.Visibility.Visible;
                    searchkeyword = keywordtb.Text.ToString();
                    sResultContainer = new YoutubeSearchResultContainer();
                    FiveArguDelegate search = new FiveArguDelegate(
                        this.SearchDataFromServer);
                    search.BeginInvoke(keywordtb.Text.ToString(), null, country, searchamount, getRepeatTolerateDays(), null, null);
                }     
            }
            else if (searchbtn.Content.Equals("Stop"))
            {
                searchbtn.Content = "Stopping";
                searchbtn.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                           new NoArguDelegate(stoptheprocess));
                searchbtn.IsEnabled = false;
            }
            
        }

        private void stoptheprocess()
        {
            stopsearch = true;
        }






        private class YoutubeSearchResultContainer
        {
            private List<string> videoTitles;
            private List<int> viewCounts;
            private List<string> channelTitles;
            private List<int> channelSubscriber;
            private List<int> averageViewcount;
            private List<string> emails;
            private List<string> videoidlink;
            private List<string> channelids;
            private string channelid;
            private List<string> existchannelids;

            //private List<string> localvideoTitles;
            //private List<int> localviewCounts;
            //private List<string> localchannelTitles;
            //private List<int> localchannelSubscriber;
            //private List<int> localaverageViewcount;
            //private List<string> localemails;
            //private List<string> localvideoidlink;
            //private string localchannelid;

            private int searchedamount;
            private int achievedamount;
            private Informationdata indata;
            


            



            public string getVideoLink(int index)
            {
                return this.videoidlink[index];
            }

            public YoutubeSearchResultContainer()
            {
              
                
            }

            public void startExtractFromSearchResult(int repeatdays)
            {
                videoTitles = new List<string>();
                viewCounts = new List<int>();
                channelTitles = new List<string>();
                channelSubscriber = new List<int>();
                averageViewcount = new List<int>();
                emails = new List<string>();
                videoidlink = new List<string>();
                channelids = new List<string>();
                searchedamount = 0;
                achievedamount = 0;
                existchannelids = new List<string>();
                indata = new Informationdata();
                importexistchannelids(repeatdays);
            }

            public void importexistchannelids(int deltadays)
            {
                string spath = Directory.GetCurrentDirectory();
                try
                {
                    foreach (string f in Directory.GetFiles(Directory.GetCurrentDirectory(),"*.bfb"))
                    {  
                        if ((DateTime.Today - Convert.ToDateTime(System.IO.Path.GetFileNameWithoutExtension(f).Replace("-", "/"))).TotalDays < deltadays)
                        {
                            StreamReader sr = new StreamReader(f);
                            while(!sr.EndOfStream)
                            existchannelids.Add(sr.ReadLine());
                        }    
                    }
                }
                catch (System.Exception excpt)
                { }
            }



            public Informationdata extractFromSearchResult(SearchResult searchResult, int searchamount, string country)
            {
                
                channelid = searchResult.Snippet.ChannelId;
                foreach (var tempchannelid in channelids)
                    if (tempchannelid == channelid)
                        return indata.infodata(++searchedamount + "", "Channel existing, skip.", achievedamount+"");
                foreach (var tempchannelid in existchannelids)
                    if(tempchannelid == channelid)
                        return indata.infodata(++searchedamount + "", "Channel existing in previous data, skip.", achievedamount + "");

                string c = GetChannelCountry(channelid);
                if (country != "ALL" && c != country )//&& !DetectLanguagebyAPI(country,searchResult.Id.VideoId)) //GB FR US
                    return indata.infodata(++searchedamount + "", "Country not satisfied.", achievedamount + ""); 
                
                channelids.Add(channelid);
                videoTitles.Add(searchResult.Snippet.Title);               
                videoidlink.Add(searchResult.Id.VideoId);                
                viewCounts.Add(GetViewCount(searchResult.Id.VideoId));               
                channelTitles.Add(searchResult.Snippet.ChannelTitle);                
                ChannelStatistics channelstatistics = GetChannelSubscribe(searchResult.Snippet.ChannelId);
                channelSubscriber.Add((int)channelstatistics.SubscriberCount);          
                averageViewcount.Add((int)(channelstatistics.ViewCount / channelstatistics.VideoCount));          
                emails.Add(ExtractEmails(searchResult.Id.VideoId));
                return indata.infodata(++searchedamount + "", "Get you!", ++achievedamount + "");
            }


            public int getListAmount() 
            {
                return channelids.Count();
            }

            public void presenteSearchResult(ListView resultLV)
            {
                int index = 0;
               
                foreach (var videotitle in  videoTitles)
                {
                    resultLV.Items.Add(new MyItem
                    {
                        
                        title = videotitle,
                        count = string.Format("{0:N0}", viewCounts[index]),
                        subscriber = string.Format("{0:N0}", channelSubscriber[index]),
                        avgviewcount = string.Format("{0:N0}", averageViewcount[index]),
                        email = emails[index],
                        channel = channelTitles[index++]
                    });
                }
                    
            }

            private int GetViewCount(string videoid)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCNXbQfrNeR76wHahW0JoTnIb4i4Xq-dzs",
                    ApplicationName = this.GetType().ToString()
                });

                var videoListRequest = youtubeService.Videos.List("statistics");
                videoListRequest.Id = videoid;
                var videoListResponse = videoListRequest.Execute();
                return (int)videoListResponse.Items[0].Statistics.ViewCount;
            }

            private bool DetectLanguagebyAPI(string country, string videoid)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCNXbQfrNeR76wHahW0JoTnIb4i4Xq-dzs",
                    ApplicationName = this.GetType().ToString()
                });

                var videoListRequest = youtubeService.Videos.List("snippet");
                videoListRequest.Id = videoid;
                var videoListResponse = videoListRequest.Execute();
                string data = videoListResponse.Items[0].Snippet.Description;
                List<string> languages = DetectLanguage.detectString(data);
                foreach (var language in languages)
                    if (country == language)
                        return true;
                return false;
            }

            private string ExtractEmails(string videoid)
            {

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCNXbQfrNeR76wHahW0JoTnIb4i4Xq-dzs",
                    ApplicationName = this.GetType().ToString()
                });

                var videoListRequest = youtubeService.Videos.List("snippet");
                videoListRequest.Id = videoid;
                var videoListResponse = videoListRequest.Execute();
                string data = videoListResponse.Items[0].Snippet.Description;
                Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*",
                    RegexOptions.IgnoreCase);
                //find items that matches with our pattern
                MatchCollection emailMatches = emailRegex.Matches(data);

                StringBuilder sb = new StringBuilder();

                foreach (Match emailMatch in emailMatches)
                {
                    sb.AppendLine(emailMatch.Value);
                }
                string sbstring = sb.ToString();
                sbstring = Regex.Replace(sbstring, @"[\n\r]", "");
                //store to file
                return sbstring;
            }

            private string GetChannelCountry(string channelid)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCNXbQfrNeR76wHahW0JoTnIb4i4Xq-dzs",
                    ApplicationName = this.GetType().ToString()
                });
                var channelListRequest = youtubeService.Channels.List("snippet");
                channelListRequest.Id = channelid;
                var channelListResponse = channelListRequest.Execute();
                try
                {
                    return channelListResponse.Items[0].Snippet.Country;
                }
                catch {
                    return "error";
                }
               
            }

            private ChannelStatistics GetChannelSubscribe(string channelid)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCNXbQfrNeR76wHahW0JoTnIb4i4Xq-dzs",
                    ApplicationName = this.GetType().ToString()
                });
                var channelListRequest = youtubeService.Channels.List("statistics");
                channelListRequest.Id = channelid;
                var channelListResponse = channelListRequest.Execute();
                return channelListResponse.Items[0].Statistics;
            }

            public void saveExcel(string filename)
            {
                DateTime thisday = DateTime.Today; 
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                StreamWriter sw = new StreamWriter(thisday.ToString("d").Replace("/","-")+".bfb",true);
                object misValue = System.Reflection.Missing.Value;

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Add(misValue);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                Excel.Range range = xlWorkSheet.Range["A1", "Z65535"];
                int ind = 2;

                xlWorkSheet.Cells[1, 1] = "Subscriber";
                xlWorkSheet.Cells[1, 2] = "Video link";
                xlWorkSheet.Cells[1, 3] = "Subscriber count";
                xlWorkSheet.Cells[1, 4] = "Average view count";
                xlWorkSheet.Cells[1, 5] = "Email";

                foreach (var videotitle in videoTitles)
                {
                    sw.WriteLine(channelids[ind - 2]);
                    xlWorkSheet.Cells[ind, 1] = channelTitles[ind - 2];
                    xlWorkSheet.Cells[ind, 2] = "https://www.youtube.com/watch?v=" + videoidlink[ind - 2];
                    // wSheet.Cells[ind, 3] = viewCounts[ind-2];
                    xlWorkSheet.Cells[ind, 3] = channelSubscriber[ind - 2];
                    xlWorkSheet.Cells[ind, 4] = averageViewcount[ind - 2];
                    xlWorkSheet.Cells[ind, 5] = emails[ind++ - 2];
                }

                sw.Close();

               // Excel.Range hyrange = xlWorkSheet.Range["B2" , "B5"];

                //hyrange.Hyperlinks.Add(hyrange, "", Type.Missing, Type.Missing, Type.Missing);

                range.EntireColumn.AutoFit();

                xlWorkBook.SaveAs(filename, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                xlWorkBook.Close(true, misValue, misValue);
                xlApp.Quit();

                releaseObject(xlWorkSheet);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);

            }

            private void releaseObject(object obj)
            {
                try
                { 
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                    obj = null;
                }
                catch (Exception ex)
                {
                    obj = null;
                    MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
                }
                finally
                {
                    GC.Collect();
                }
            }

        }


        private void SearchDataFromServer(string keywd, string nextpagetoken, string country, string searchamount, int repeatdays)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCNXbQfrNeR76wHahW0JoTnIb4i4Xq-dzs",
                ApplicationName = this.GetType().ToString()
            });

            int samount = int.Parse(searchamount);
            // Items.ToString();
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = keywd; // Replace with your search term.
            searchListRequest.MaxResults = searchamountperrequest;
            
            // Call the search.list method to retrieve results matching the specified query term.
           
            
           

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            sResultContainer.startExtractFromSearchResult(repeatdays);

           
            //searchbtn.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,)
            while (sResultContainer.getListAmount() < int.Parse(searchamount) && !stopsearch )
            {
                searchListRequest.PageToken = nextpagetoken;
                var searchListResponse = searchListRequest.Execute();// .ExecuteAsync();
               
                                foreach (var searchResult in searchListResponse.Items)
                {
                    if (sResultContainer.getListAmount() >= int.Parse(searchamount) || stopsearch)
                        break;
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                           // Application.Current.Dispatcher.Invoke(()=>_a)
                            searchbtn.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, 
                                new UpdateProgress(UpdateInformation), sResultContainer.extractFromSearchResult(searchResult, samount, country));
                            break;
                    }
                }
                nextpagetoken = searchListResponse.NextPageToken;
            }


            

            // Schedule the update function in the UI thread.
            searchbtn.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new UpdateViewDelegate(UpdateUserInterface));
        }

        private void UpdateInformation(Informationdata indata)
        {
            inforLB4.Content = indata.searched;
            inforLB5.Content = indata.category;
            inforLB6.Content = indata.achieved;
        }

        private void UpdateUserInterface()
        {
            if (newsearch)
                resultList.Items.Clear();
            
            sResultContainer.presenteSearchResult(resultList);
            newsearch = false;

            searchbtn.Content = "Search";
            searchbtn.IsEnabled = true;
            defreezebtn();
            RealtimeinfoVB.Visibility = System.Windows.Visibility.Hidden;
            
            foreach (GridViewColumn c in resultgrid.Columns)
            {
                c.Width = 0; //set it to no width
                c.Width = double.NaN; //resize it automatically
            }
            stopsearch = false;
        }

        public class MyItem
        {
           
            public string title { get; set; }
            public string count { get; set; }
            public string channel { get; set; }
            public string subscriber { get; set; }
            public string avgviewcount { get; set; }
            public string email { get; set; }
        }

        private void resultList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (resultlistboxclickable)
                System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=" + sResultContainer.getVideoLink(resultList.SelectedIndex));

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           //resultList.Width = this.Width-100;
            //resultList.Height = (this.Height-200);

        }

        private void freezebtn()
        {
           
            loadmorebtn.IsEnabled = false;
            loadmorebtn.Content = "Loading..";
            Export.IsEnabled = false;
            countryCB.IsEnabled = false;
            searchamountTB.IsEnabled = false;
            resultList.IsEnabled = false;
            keywordtb.IsEnabled = false;
            repeatTolerateCB.IsEnabled = false;
            
        }

        private void defreezebtn()
        {
            resultlistboxclickable = true;
            loadmorebtn.IsEnabled = true;
            loadmorebtn.Content = "Load more";
            Export.IsEnabled = true;
            Export.Content = "Export to Excel";
            countryCB.IsEnabled = true;
            searchamountTB.IsEnabled = true;
            resultList.IsEnabled = true;
            keywordtb.IsEnabled = true;
            repeatTolerateCB.IsEnabled = true;
        }


        private void loadmorebtn_Click(object sender, RoutedEventArgs e)
        {
            //freezebtn();
            //scroviewinformationtext.Visibility = System.Windows.Visibility.Visible;
            //newsearch = false;
            //SearchDataFromServerDelegate search = new SearchDataFromServerDelegate(
            //    this.SearchDataFromServer);
            //search.BeginInvoke(keywordtb.Text.ToString(), nextpgtoken, null, null);
        }

        private void Excelbtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Excel files|*.xls", DefaultExt = "xls" };
            freezebtn();
            searchbtn.IsEnabled = false;
            
            if (saveFileDialog.ShowDialog() == true)
            {
                sResultContainer.saveExcel(saveFileDialog.FileName);
                MessageBox.Show("The Save button was clicked or the Enter key was pressed" +
                                "\nThe file would have been saved as " +
                                saveFileDialog.FileName);
            }
            else
                MessageBox.Show("The Cancel button was clicked or Esc was pressed");
            searchbtn.IsEnabled = true;
            searchbtn.Content = "Search";
            defreezebtn();

        }

        

       

        private void keywordtb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                button_Click(sender, e);
            }
        }

        private void searchamountTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

       

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }

        private void searchbtn_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            ExitBtn.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Send);
        }

        private void label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", @Directory.GetCurrentDirectory());
        }



        //private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    clicado = false;
        //}

        //private void Window_MouseMove(object sender, MouseEventArgs e)
        //{

        //    if (clicado)
        //    {
        //        Point MousePosition = e.GetPosition(this);
        //        this.Left += (MousePosition.X - this.lm.X);
        //        this.Top += (MousePosition.Y - this.lm.Y);
        //        this.lm = MousePosition;
        //    }
        //}
    }
}
