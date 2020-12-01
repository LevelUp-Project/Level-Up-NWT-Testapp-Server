
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace NWTServer
{
    /*
    Row 24-292
    Definitions of Database tables
    */
    [Table("Users")] //The data representing a user in the app. 
    public class UserTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int Age { get; set; }
        public int Plustokens { get; set; } //Represent how much currency the user has in the app
        public string AchievementString { get; set; } //JSONstring used to represent the users progress on Achievements.
        public string MissionString { get; set; } //JSONstring used to represent the users progress on daily missions.
        public string TaggString { get; set; } //JSONstring used to represent the taggs the user follows.
        public int LoginStreak { get; set; } //How many days the users has logged in consecutivly
        public int DailyLogin { get; set; } // Boolean to check if the user has logged in today or not.
        public int TutorialProgress { get; set; } //A number 1-5 to check on what stage on the tutorial
        public string Inventory { get; set; } //JSONstring representing what items from the InAppStore the user has bought
        public string Avatar { get; set; } //JSONstring representing what avatar items is used to display their avatar
        public string Style { get; set; } //What background color the user has set to their default
    }

    [Table("Tokens")] // Table that holds the loginToken for all logged in users.
    public class TokenTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public int User { get; set; }       
        public string Token { get; set; } 
        public string IP { get; set; }
        public string LastUse { get; set; }
    }

    [Table("Comments")] // Data that represents all comments users posts to articles
    public class CommentTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public long Article { get; set; } //ID of a RSSTable // UserRSSTable 
        public int UserSubmitted { get; set; } //If the article is RSS/UserRSS Table
        public int CommentNR { get; set; }
        public int User { get; set; } //ID of a UserTable
        public string Comment { get; set; }
        public int Point { get; set; } //Represents the (Likes - Dislikes) the comments have
        public int Replynr { get; set; } 
        public int Replylvl { get; set; } //The Depth of the comment chain the comment is posted in. 0 = Top Level.
    }

    [Table("Sudoku")] //Data that represents a sudoku puzzle, made from 2 JSONstring, one with the answer, the other with the preplaced numbers.
    public class SudokuTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public string Difficulty { get; set; }
        public string ValueList { get; set; }
        public string PlacedList { get; set; }
    }

    [Table("RSS")] //Data the represents an article extracted from the RSS feed.
    public class RSSTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PubDate { get; set; }
        public string Link { get; set; }
        public string Source { get; set; }
        public int Plus { get; set; }
        public int NewsScore { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
        public string Content { get; set; }
        public string ImgSource { get; set; }
        public string Ordning { get; set; } //List of Numbers used to determine the order of texts and images
        public string Text { get; set; } // The body Text split in a list to determine sections to be placed according to the ordning.
        public string Images { get; set; }// Images in the body whose placement is determined by the ordning.
        public string Imagetext { get; set; } //Text to place under the above images

    }

    [Table("Insandare")]// Data representing a user submitted article.
    public class UserRSSTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public string Rubrik { get; set; }
        public string Ingress { get; set; }
        public string Brodtext { get; set; }
        public long Referat { get; set; }
        public DateTime PubDate { get; set; }
        public int Author { get; set; }
    }

    [Table("TaskList")] //The data used to make the users Mission string.
    public class Task
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public int Progress { get; set; }
        public int Goal { get; set; }
        public int Completed { get; set; }
        public string Type { get; set; }
        public int Mission { get; set; }
        public double Modifier { get; set; }
    }

    [Table("Plus")] //Table that hold the information if a user has bought access to a Plus Locked article.
    public class PlusRSSTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public long Article { get; set; }
        public int User { get; set; }
    }

    [Table("Stats")] // Data that checks how many times a user has done certain specific actions.
    public class StatsTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public int User { get; set; }
        public int Logins { get; set; }
        public int UseTime { get; set; }
        public int ArticlesRead { get; set; }
        public int PlusArticlesUnlocked { get; set; }
        public int InsandareSubmitted { get; set; }
        public int InsandareRead { get; set; }
        public int GameFinished { get; set; }
        public int QuestionSubmitted { get; set; }
        public int QuestionAnswered { get; set; }
        public int VoteQuestionSubmitted { get; set; }
        public int VoteSubmitted { get; set; }
        public int CommentsPosted { get; set; }
        public int TokensCollected { get; set; }
        public int StylesChanged { get; set; }
        public int AvatarChanged { get; set; }
        public int BoughtItems { get; set; }
        public int BoughtStyles { get; set; }
        public int CoinsSpent { get; set; }
        public int ArticlesClicked { get; set; }
        public int SubjectClicked { get; set; }
        public int SubjectAdded { get; set; }
        public int SubjectRemoved { get; set; }
        public int GenericStat1 { get; set; }
        public int GenericStat2 { get; set; }
        public int GenericStat3 { get; set; }
        public int GenericStat4 { get; set; }
        public int GenericStat5 { get; set; }
    }

    [Table("Questions")] // Data representing a Question used for the apps Quiz game.
    public class QuizTable 
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public string Category { get; set; }
        public string QuestionText { get; set; }
        public string ChoiceA { get; set; }
        public string ChoiceB { get; set; }
        public string ChoiceC { get; set; }
        public string ChoiceD { get; set; }
        public string CorrectAnswer { get; set; }
    }

    [Table("Favorites")] // Data showing whats RSSTables/UserRSSTables the user has favorited
    public class FavoritesTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public long Article { get; set; }
        public int User { get; set; }
        public string Header { get; set; }
        public string Image { get; set; }
    }

    [Table("History")]  // Data showing whats RSSTables/UserRSSTables the user has most recently read
    public class HistoryTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public long Article { get; set; }
        public int User { get; set; }
        public string Header { get; set; }
        public string Image { get; set; }
        public DateTime Readat { get; set; }
    }

    [Table("Reaction")] // Data representing a Reaction a User has put on an article.
    public class ReactionTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public long Article { get; set; }
        public int User { get; set; }
        public int Reaktion { get; set; }
    }


    [Table("Newsfeed")] // A more compressed version of a RSStable used to display an article in an "infinite Scroll"
    public class NewsfeedTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public long Article { get; set; }
        public int NewsScore { get; set; }
        public string Header { get; set; }
        public string Ingress { get; set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
        public DateTime DatePosted { get; set; }
        public int Plus { get; set; }
        public string ArtikelReactions { get; set; }
        public string ReactionSum { get; set; }


    }

    [Table("VoteQuestions")] //Data used to represent the "Timed Poll" feature used in the App
    public class VoteQuestionTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public string Question { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public DateTime Posted { get; set; }
        public int Stage { get; set; } // 0 = Poll Submited, 1 = Poll Running, 2 = Poll Presenting Results, 3 = Poll Archived
        public int Winner { get; set; } // 1-4, the option that won when the timer expired
        public int TotalVotes1 { get; set; }
        public int TotalVotes2 { get; set; }
        public int TotalVotes3 { get; set; }
        public int TotalVotes4 { get; set; }

    }
    [Table("Votes")] // Data used to represent the votes for a question in the "Timed Poll" feature used in the app.
    public class VoteTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public int User { get; set; }
        public int Question { get; set; }
        public int ChoosenOption { get; set; }
    }

    [Table("Picross")] // Data used to represent a picross puzzle.
    public class PicrossTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
        public string Left { get; set; } // JSONstring of the helping numbers in the left row.
        public string Top { get; set; } // JSONstring of the helping numbers in the top row.
        public string Gameboard { get; set; } // JSONstring of the data of the finished picture
    }


    [Table("Test")] //Table used for testing database connections.
    public class TestTable
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int ID { get; set; }
    }

    public class DBHelper
    {
        private static ManualResetEvent _resetEvent = new ManualResetEvent(false);
        public static System.Timers.Timer Timer;
        public static System.Timers.Timer SynchroTimer;
        int lastHour = DateTime.Now.Hour;
        static SQLiteConnection DB;
        bool Init = false;  
        
        public DBHelper(string dbPath) //Constructor for the DBHelper, takes a path to a SQLlite database as input
        {
            DB = new SQLiteConnection(dbPath);
            
            DB.DropTable<TokenTable>();// Tokens are refreshed on startup
            DB.CreateTable<TokenTable>();

            

            Timer = new System.Timers.Timer(5000); // Timer used for startup command
            Timer.Elapsed += OnTimedEvent;

            SynchroTimer = new System.Timers.Timer(1000*60); //Timer used for database events.
            SynchroTimer.Elapsed += SyncEvent;

            _resetEvent.Reset();


            Console.WriteLine("Database Startup, type 'init' to go to table clearing, press enter to resume startup."); // If some or all tables needs to be reinitialized, the user can type 'init' within the first 5 seconds of startup, otherwise the program will start as normal.
            string input = "";

            Timer.Start();
            new Thread(() =>
            {
                while (true)
                {
                    
                    input = Console.ReadLine();
                    _resetEvent.Set();
                    Timer.Stop();
                }
            }).Start();

            _resetEvent.WaitOne();


            if (input == "init")
            {
                Console.WriteLine("Table clearing initialized");
                Console.WriteLine("Rebuild Newsfeed?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    DB.DropTable<NewsfeedTable>();
                    DB.CreateTable<NewsfeedTable>();
                    DB.DropTable<ReactionTable>();
                    DB.CreateTable<ReactionTable>();
                    NewsfeedRebuild();
                    Init = true;
                    Console.WriteLine("Articles Cleared");
                }

                Console.WriteLine("Clear Article Related Newsfeeds?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    DB.DropTable<PlusRSSTable>();
                    DB.CreateTable<PlusRSSTable>();
                    DB.DropTable<FavoritesTable>();
                    DB.CreateTable<FavoritesTable>();
                    DB.DropTable<HistoryTable>();
                    DB.CreateTable<HistoryTable>();
                    DB.DropTable<NewsfeedTable>();
                    DB.CreateTable<NewsfeedTable>();
                    DB.DropTable<ReactionTable>();
                    DB.CreateTable<ReactionTable>();
                    NewsfeedRebuild();
                    Init = true;
                    Console.WriteLine("Articles Cleared");
                }
                Console.WriteLine("Clear Users?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    DB.DropTable<UserTable>();
                    DB.CreateTable<UserTable>();
                    DB.DropTable<PlusRSSTable>();
                    DB.CreateTable<PlusRSSTable>();
                    DB.DropTable<StatsTable>();
                    DB.CreateTable<StatsTable>();
                    DB.DropTable<FavoritesTable>();
                    DB.CreateTable<FavoritesTable>();
                    DB.DropTable<HistoryTable>();
                    DB.CreateTable<HistoryTable>();
                    DB.DropTable<CommentTable>();
                    DB.CreateTable<CommentTable>();
                    Init = true;
                    Console.WriteLine("Users Cleared");
                }
                Console.WriteLine("Clear Games?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    Init = true;
                    DB.DropTable<SudokuTable>();
                    DB.CreateTable<SudokuTable>();
                    MakeSudoku();
                    DB.DropTable<PicrossTable>();
                    DB.CreateTable<PicrossTable>();
                    MakePicross();
                    Console.WriteLine("Games Cleared");

                }
                Console.WriteLine("Clear Extrafeeds?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    DB.DropTable<FavoritesTable>();
                    DB.CreateTable<FavoritesTable>();
                    DB.DropTable<HistoryTable>();
                    DB.CreateTable<HistoryTable>();
                    Console.WriteLine("Extrafeeds Cleared");
                }
                Console.WriteLine("Clear Comments?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    DB.DropTable<CommentTable>();
                    DB.CreateTable<CommentTable>();
                    Console.WriteLine("Comments Cleared");
                }
                Console.WriteLine("Clear Userarticles?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    Init = true;
                    Console.WriteLine("Userarticles Cleared");
                }
                Console.WriteLine("Clear VoteQuestions?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    DB.DropTable<VoteQuestionTable>();
                    DB.CreateTable<VoteQuestionTable>();
                    DB.DropTable<VoteTable>();
                    DB.CreateTable<VoteTable>();
                    Console.WriteLine("VoteQuestions Cleared");
                }
                Console.WriteLine("Reset Stats?");
                input = Console.ReadLine();
                if (input == "y")
                {
                    DB.DropTable<StatsTable>();
                    DB.CreateTable<StatsTable>();
                    Console.WriteLine("StatsTable Cleared");
                }
            }
            if (Init)
            {
                DB.DropTable<UserRSSTable>();
                DB.CreateTable<UserRSSTable>();

                var Dummy = new UserRSSTable();
                Dummy.Author = -1;
                Dummy.Rubrik = "Instruktioner för insändare";
                Dummy.Ingress = "Detta är en insändare, som i denna app fungerar som användarinlägg.";
                Dummy.Brodtext = "Du som användare kan skriva dina egna inlägg, och låta andra användare kommentera på den. Pröva gärna själv!";
                Dummy.PubDate = DateTime.Now;
                Dummy.Referat = -1;
                Insert(Dummy);

                Dummy.Rubrik = "Instruktioner för insändare 2";
                Dummy.Ingress = "Insändare kan användas för att skriva ett helt eget inlägg om något ämne eller som ett svar eller diskussion om en vanlig artikel.";
                Dummy.Brodtext = "Man kan använda refereatknappen i insändarskrivaren om man vill ge en länk till artikeln man skriver en insändare om.";
                Insert(Dummy);

                Dummy.Rubrik = "Bostadsförsäkringar";
                Dummy.Ingress = "";
                Dummy.Brodtext = "Hej! Mitt namn är Jakob Karlsson, och jag letar efter bostadsförsäkring till mitt nya hus. Jag byggd den själv ute i skogen utanför Malmö, väldigt stolt! Enligt hemsidor på nätet så kommer det kosta ca. 3000kr per månad lägst om jag ska försäkra den på ett försäkringsbolag. Är det möjligt att be någon försäkra det åt en? Det vill säga, kan jag ge nin kompis Bosse 300kr i veckan för att hålla ögonen på min stuga, eller måste jagskaffa något officiellt ? ";
                Insert(Dummy);

                Dummy.Rubrik = "Telefon kapad?";
                Dummy.Ingress = "Samma nummer har ringts upp från MIN telefon x antal gånger, alltså vi snackar 10 ~ 15 tillfällen. Kan nån annan ringa genom mitt nummer på något sätt? Via min gmail eller nån annan app ? ";
                Dummy.Brodtext = "";
                Insert(Dummy);

                Dummy.Rubrik = "Händer det något spännande på nyår i Skövde?";
                Dummy.Ingress = "Jag har bott här i någon år nu (student) och jag kommer fira nyår här med några vänner.";
                Dummy.Brodtext = "Finns det något kul ställe att hänga på över midnatt, eller något coolt evenemang?";
                Insert(Dummy);

                DB.DropTable<QuizTable>();
                DB.CreateTable<QuizTable>();

                var Quest = new QuizTable();

                Quest.QuestionText = "Vad är (40+3*5)/5 ?";
                Quest.Category = "Matematik";
                Quest.ChoiceA = "11";
                Quest.ChoiceB = "23";
                Quest.ChoiceC = "43";
                Quest.ChoiceD = "215";
                Quest.CorrectAnswer = "A";
                Insert(Quest);

                Quest.QuestionText = "Vilket tröjnummer hade hockeyspelaren Peter Forsberg när han spelade";
                Quest.Category = "Sport";
                Quest.ChoiceA = "13";
                Quest.ChoiceB = "17";
                Quest.ChoiceC = "21";
                Quest.ChoiceD = "99";
                Quest.CorrectAnswer = "C";
                Insert(Quest);

                Quest.QuestionText = "Vilket år blev Gustav Vasa kung?";
                Quest.Category = "Historia";
                Quest.ChoiceA = "1496";
                Quest.ChoiceB = "1523";
                Quest.ChoiceC = "1531";
                Quest.ChoiceD = "1560";
                Quest.CorrectAnswer = "B";
                Insert(Quest);

                Quest.QuestionText = "Med vilken låt vann Carola Eurovision Song Contest?";
                Quest.Category = "Musik";
                Quest.ChoiceA = "Främling";
                Quest.ChoiceB = "One Love";
                Quest.ChoiceC = "Evighet";
                Quest.ChoiceD = "Fångad av en Stormvind";
                Quest.CorrectAnswer = "D";
                Insert(Quest);

                Quest.QuestionText = "Vem är Sveriges nuvarande statsminster?";
                Quest.Category = "Politik";
                Quest.ChoiceA = "Stefan Lövfen";
                Quest.ChoiceB = "Fredrik Reinfeldt";
                Quest.ChoiceC = "Göran Persson";
                Quest.ChoiceD = "Carl Bildt";
                Quest.CorrectAnswer = "A";
                Insert(Quest);

                Quest.QuestionText = "Vilken är Schweiz huvudstad";
                Quest.Category = "Geografi";
                Quest.ChoiceA = "Wien";
                Quest.ChoiceB = "Zurich";
                Quest.ChoiceC = "Bern";
                Quest.ChoiceD = "Hamburg";
                Quest.CorrectAnswer = "C";
                Insert(Quest);

            }
            DB.DropTable<TestTable>();
            DB.CreateTable<TestTable>();
            SynchroTimer.Enabled = true;
        }


       
        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e) // Event that happens after the 5sec wait for whether initalization happens or not.
        {
            _resetEvent.Set();
            Timer.Stop();
        }

        private void SyncEvent(object source, System.Timers.ElapsedEventArgs e) // Event that happens every 60 seconds.
        {
            Console.WriteLine("Synchro Event");

            var TT = DB.Query<TokenTable>("SELECT * FROM Tokens");
            foreach (TokenTable T in TT) //Ups the UseTime stat for all logged in users.
            {
                Stats("UseTime", T.User);
            }


            if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0)) // Events that happen every hour.
            {                
                Console.WriteLine("New Hour, Searching for Articles");
                ParseRssFile(); // Contacts the RSS feed to see if there are any new articles.
                if(lastHour == 23 && DateTime.Now.Hour == 0)
                {
                    Console.WriteLine("New Day, Updating Streaks.");
                    Midnight(); 
                }
                lastHour = DateTime.Now.Hour;
            }
            else if(DateTime.Now.Minute == 30) // Every half hour the server reboots to avoid filling up memory.
            {
                System.Environment.Exit(1);
            }

        }

        public void Midnight() // Event that happens at 0000
        {
            var Users = DB.Query<UserTable>("SELECT * FROM Users");



            foreach(UserTable User in Users)
            {
                if(User.DailyLogin == 1) // For every user that logged in today, reset daily login, otherwise reset their loginstreaks.
                {
                    DB.Execute("UPDATE Users SET DailyLogin = '" + 0 + "' WHERE ID = " + User.ID);
                }
                else
                {
                    DB.Execute("UPDATE Users SET LoginStreak = '" + 0 + "' WHERE ID = " + User.ID);
                }
                Mission("Reset", User.ID); //Reset all users missions
                
            }
            VoteTick(); 
        }

        public void VoteTick() //Updates all Timed votes to the next stage.
        {
            var Questions = DB.Query<VoteQuestionTable>("SELECT * FROM VoteQuestions");
            foreach (VoteQuestionTable VQ in Questions)
            {
                if (VQ.Stage == 0) //Sets a submitted vote as Active
                {
                    DB.Execute("UPDATE VoteQuestions SET Stage = '" + 1 + "' WHERE ID = " + VQ.ID);
                }
                else if(VQ.Stage == 1) // Takes the results of an active vote and sets the total votes and winners and making it go to "Results"
                {
                    var Q1 = VoteQuery("SELECT * FROM Votes WHERE Question = " + VQ.ID + " AND ChoosenOption = 1").Count();
                    var Q2 = VoteQuery("SELECT * FROM Votes WHERE Question = " + VQ.ID + " AND ChoosenOption = 2").Count();
                    var Q3 = VoteQuery("SELECT * FROM Votes WHERE Question = " + VQ.ID + " AND ChoosenOption = 3").Count();
                    var Q4 = VoteQuery("SELECT * FROM Votes WHERE Question = " + VQ.ID + " AND ChoosenOption = 4").Count();
                    var QMax = Math.Max(Q1, Math.Max(Q2, Math.Max(Q3, Q4)));
                    DB.Execute("UPDATE VoteQuestions SET TotalVotes1 = '" + Q1 + "' WHERE ID = " + VQ.ID);
                    DB.Execute("UPDATE VoteQuestions SET TotalVotes2 = '" + Q2 + "' WHERE ID = " + VQ.ID);
                    DB.Execute("UPDATE VoteQuestions SET TotalVotes3 = '" + Q3 + "' WHERE ID = " + VQ.ID);
                    DB.Execute("UPDATE VoteQuestions SET TotalVotes4 = '" + Q4 + "' WHERE ID = " + VQ.ID);
                    DB.Execute("UPDATE VoteQuestions SET Stage = '" + 2 + "' WHERE ID = " + VQ.ID);
                }
                else if (VQ.Stage == 2) // Take a VoteQuestion in the Results stage, removes all of the votes from the question from the database and archives it.
                {
                    DB.Execute("DELETE FROM Votes WHERE Question = '" + VQ.ID + "'");
                    DB.Execute("UPDATE VoteQuestions SET Stage = '" + 3 + "' WHERE ID = " + VQ.ID);
                }
            }
        }

        public static List<Task> MissionStringGen(UserTable User) //Randomizes the daily missions for the users
        {
            List<string> Mission1List = new List<string>() { "ArticlesRead", "ArticlesRead", "InsandareRead", "ArticlesRead", "ArticlesRead", "InsandareRead", "ArticlesRead", "ArticlesRead", "InsandareRead", "ArticlesRead" };
            List<string> Mission2List = new List<string>() { "CommentsPosted", "QuestionSubmitted", "CommentsPosted", "VoteQuestionSubmitted", "CommentsPosted", "QuestionSubmitted", "CommentsPosted", "VoteQuestionSubmitted", "CommentsPosted", "InsandareSubmitted" };
            List<string> Mission3List = new List<string>() { "GameFinished", "QuestionAnswered", "VoteSubmitted", "QuestionAnswered", "VoteSubmitted", "QuestionAnswered", "VoteSubmitted", "QuestionAnswered", "VoteSubmitted", "GameFinished" };

            List<Task> TaskList = new List<Task>();
            Random rnd = new Random();

            Task M1 = new Task
            {
                ID = 1,
                Progress = 0,
                Type = Mission1List[rnd.Next(0, Mission1List.Count - 1)],
                Goal = 5 + rnd.Next(10),
                Mission = 1,
                Completed = 0,
                Modifier = 2.5 
            };
            TaskList.Add(M1);

            Task M2 = new Task
            {
                ID = 2,
                Progress = 0,
                Type = Mission2List[rnd.Next(0, Mission2List.Count - 1)],
                Mission = 2,
                Completed = 0
            };
                if (M2.Type == "CommentsPosted")
                {
                    M2.Goal = 1 + rnd.Next(1);
                    M2.Modifier = 0.5;
                }
                else if(M2.Type == "InsandareRead")
                {
                    M2.Goal = 1;
                    M2.Modifier = 0.125;
                }
                else if (M2.Type == "VoteQuestionSubmitted")
                {
                    M2.Goal = 1;
                    M2.Modifier = 0.25;
                }
                else if (M2.Type == "QuestionSubmitted")
                {
                    M2.Goal = 1 + rnd.Next(1);
                    M2.Modifier = 0.333333;
                }
                else
                {
                    M2.Goal = 1;
                    M2.Modifier = 1;
                }
            TaskList.Add(M2);

            Task M3 = new Task
            {
                ID = 3,
                Progress = 0,
                Type = Mission1List[rnd.Next(0, Mission1List.Count - 1)],
                Goal = 5 + rnd.Next(10),
                Mission = 1,
                Completed = 0,
                Modifier = 2.5

                /*
                Progress = 0,
                Type = Mission3List[rnd.Next(0, Mission3List.Count - 1)],
                Mission = 3,
                Completed = 0*/
            };
                if (M3.Type == "GameFinished")
                {
                    M3.Goal = 1 + rnd.Next(1);
                    M3.Modifier = 0.2;
                }
                else if (M3.Type == "QuestionAnswered")
                {
                    M3.Goal = 5 + rnd.Next(5);
                    M3.Modifier = 1;
                }
                else if (M3.Type == "VoteSubmitted")
                {
                    M3.Goal = 2 + rnd.Next(2);
                    M3.Modifier = 0.5;
                }
                else
                {

                }
            TaskList.Add(M3);

            return TaskList;
        }

        public void Execute(string statement) // Executes the inputed SQL statement
        {
            Console.WriteLine("Execute");
            Console.WriteLine(statement);
            var EM = DB.Execute(statement);
            Console.WriteLine(EM);
        }
        public void Insert<T>(T arg) // Insert a Table object of the Type T
        {
            Console.WriteLine("Insert");
            Console.WriteLine(arg.ToString());
            var EM = DB.Insert(arg);
  
            Console.WriteLine(EM);
        }
        public void Delete<T>(T arg) // Deletes a Table object of the Type T
        {
            Console.WriteLine("Delete");
            Console.WriteLine(arg.ToString());
            var EM = DB.Delete(arg);
            Console.WriteLine(EM);
        }

        public void Stats(string Value, int ID) // Updates a stat value for a specific user.
        {
            Console.WriteLine("Stats");
            Console.WriteLine(Value);
            var EM = DB.Execute("UPDATE Stats SET " + Value + " = " + Value + " + 1 WHERE User = " + ID);
           
            Console.WriteLine(EM);
            Mission(Value,ID);
        }

        public string GetMissionString(int ID) // Returns a users MissionString
        {
            return DB.Query<UserTable>("SELECT * FROM Users WHERE ID = " + ID).First().MissionString;
        }


        public void Mission(string Value, int ID) // Updates a users Mission string on progress and rewards tokens if the mission is completed.
        {
            Console.WriteLine("Mission");
            Console.WriteLine(Value);
            UserTable User = DB.Query<UserTable>("SELECT * FROM Users WHERE ID = "+ID).First();
            List<Task> Tasklist;

            string Action = Value;

            if (Action == "Reset")
            {
                Tasklist = MissionStringGen(User);
            }
            else
            {
                Tasklist = JsonConvert.DeserializeObject<List<Task>>(User.MissionString);
                foreach (Task T in Tasklist)
                {

                    switch (Action)
                    {
                        case "Evaluate":
                            if (T.Progress >= T.Goal)
                            {
                                T.Completed = 1;
                                User.Plustokens = User.Plustokens + Convert.ToInt32(T.Goal/T.Modifier);
                                T.Progress = T.Goal;
                            }
                            break;
                        case "ArticlesRead":
                            if (T.Type == "ArticlesRead" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "InsandareRead":
                            if (T.Type == "InsandareRead" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "CommentsPosted":
                            if (T.Type == "CommentsPosted" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "QuestionSubmitted":
                            if (T.Type == "QuestionSubmitted" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "VoteQuestionSubmitted":
                            if (T.Type == "VoteQuestionSubmitted" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "InsandareSubmitted":
                            if (T.Type == "InsandareSubmitted" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "GameFinished":
                            if (T.Type == "GameFinished" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "QuestionAnswered":
                            if (T.Type == "QuestionAnswered" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                        case "VoteSubmitted":
                            if (T.Type == "VoteSubmitted" && T.Progress <= T.Goal && T.Completed == 0)
                            {
                                T.Progress++;
                            }
                            break;
                    }
                }
            }
            string Missionstring = JsonConvert.SerializeObject(Tasklist);
            string Statement = "UPDATE Users SET MissionString = '" + Missionstring + "', PlusTokens = " + User.Plustokens + " WHERE ID = " + User.ID;
            var EM = DB.Execute(Statement);

            Console.WriteLine(EM);

        }

        public void NewsfeedRebuild() //Makes a NewsfeedObject for every RSSTable object in the database based on its data.
        {
            var RSSList = DB.Query<RSSTable>("SELECT * FROM RSS");

            var AR = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < 9; i++)
            {
                AR.Add(new KeyValuePair<int, string>(i, "reactions_" + i + ".png"));
            }



            foreach (RSSTable RSS in RSSList)
            {
                var NF = new NewsfeedTable();

                NF.Article = RSS.ID;
                NF.NewsScore = RSS.NewsScore;
                NF.Header = RSS.Title;
                NF.Image = RSS.ImgSource;
                NF.Plus = RSS.Plus;
                NF.Author = RSS.Author;
                NF.Category = RSS.Category;
                NF.Tag = RSS.Tag;
                NF.Ingress = RSS.Description;
                NF.DatePosted = RSS.PubDate;
                NF.ArtikelReactions = JsonConvert.SerializeObject(AR);
                NF.ReactionSum = JsonConvert.SerializeObject(new List<ReactionTable>());
                Insert(NF);
            }
        }



        public List<UserTable> UserQuery(string statement) // Preforms a SQLlite Query that returns a UserTable Object
        {
            return DB.Query<UserTable>(statement);
        }
        public List<CommentTable> CommentQuery(string statement)// Preforms a SQLlite Query that returns a CommentTable Object
        {
            return DB.Query<CommentTable>(statement);
        }
        public List<RSSTable> RSSQuery(string statement)// Preforms a SQLlite Query that returns a RSSTable Object
        {
            return DB.Query<RSSTable>(statement);
        }
        public List<UserRSSTable> UserRSSQuery(string statement)// Preforms a SQLlite Query that returns a UserRSSTable Object
        {
            return DB.Query<UserRSSTable>(statement);
        }
        public List<TokenTable> TokenQuery(string statement)// Preforms a SQLlite Query that returns a TokenTable Object
        {
            return DB.Query<TokenTable>(statement);
        }
        public List<SudokuTable> SudokuQuery(string statement)// Preforms a SQLlite Query that returns a SudokuTable Object
        {
            return DB.Query<SudokuTable>(statement);
        }
        public List<PlusRSSTable> PlusQuery(string statement)// Preforms a SQLlite Query that returns a PlusRSSTable Object
        {
            return DB.Query<PlusRSSTable>(statement);
        }
        public List<StatsTable> StatsQuery(string statement)// Preforms a SQLlite Query that returns a StatsTable Object
        {
            return DB.Query<StatsTable>(statement);
        }
        public List<QuizTable> QuizQuery(string statement)// Preforms a SQLlite Query that returns a QuizTable Object
        {
            return DB.Query<QuizTable>(statement);
        }
        public List<FavoritesTable> FavoriteQuery(string statement)// Preforms a SQLlite Query that returns a FavoritesTable Object
        {
            return DB.Query<FavoritesTable>(statement);
        }
        public List<HistoryTable> HistoryQuery(string statement)// Preforms a SQLlite Query that returns a HistoryTable Object
        {
            return DB.Query<HistoryTable>(statement);
        }
        public List<NewsfeedTable> NewsfeedQuery(string statement)// Preforms a SQLlite Query that returns a NewsfeedTable Object
        {
            return DB.Query<NewsfeedTable>(statement);
        }
        public List<VoteQuestionTable> VoteQuestionQuery(string statement)// Preforms a SQLlite Query that returns a VoteQuestionTable Object
        {
            return DB.Query<VoteQuestionTable>(statement);
        }
        public List<VoteTable> VoteQuery(string statement)// Preforms a SQLlite Query that returns a VoteTable Object
        {
            return DB.Query<VoteTable>(statement);
        }
        public List<PicrossTable> PicrossQuery(string statement)// Preforms a SQLlite Query that returns a PicrossTable Object
        {
            return DB.Query<PicrossTable>(statement);
        }
        public List<ReactionTable> ReactionQuery(string statement)// Preforms a SQLlite Query that returns a ReactionTable Object
        {
            return DB.Query<ReactionTable>(statement);
        }
        public void MakeSudoku() // Function that makes the data for 7 included sudoku puzzles
        {
                string SV1 = "435269781682571493197834562826195347374682915951743628519326874248957136763418259";
                string SP1 = "000110101110010010110001100110100010001101100010001011001100011010010011101011000";
                string SV2 = "534678912672195348198342567859761423426853791713924856961537284287419635345286179";
                string SP2 = "110010000100111000011000010100010001100101001100010001010000110000111001000010011";
                string SV3 = "827154396965327148341689752593468271472513689618972435786235914154796823239841567";
                string SP3 = "010001100100010001000101010000000001011101110100000000010101000100010001001100010";
                string SV4 = "916247358238956174457138962521863497389472516764591283645389721892714635173625849";
                string SP4 = "111110000110000000000101001111000001011000110110010001000010111010001101011001000";
                string SV5 = "892573614746921835315468972457682391968135247123749568271896453539214786684357129";
                string SP5 = "111001011000000000000011010110010001001000100101100100011001010101100010100001001";
                string SV6 = "276314958854962713913875264468127395597438621132596487325789146641253879789641532";
                string SP6 = "100100000101011001011100100000010110101000111011001000010001110101110101000001001";
                string SV7 = "152489376739256841468371295387124659591763428246895713914637582625948137873512964";
                string SP7 = "100111001110010010110001111111110100101101001011011110111100010010010011101111001";

            List<string> SVList = new List<string>() {SV1, SV2, SV3, SV4, SV5, SV6, SV7 };
            List<string> SPList = new List<string>() {SP1, SP2, SP3, SP4, SP5, SP6, SP7 };

            for (int k = 0; k < 7; k++)
            {
                string SudokuValue = SVList[k];
                string SudokuPlacement = SPList[k];

                var BoardValue = new List<List<int>>();
                var BoardPlacement = new List<List<int>>();
                for (int i = 0; i < 9; i++)
                {
                    var Temp = new List<int>();
                    for (int j = 0; j < 9; j++)
                    {

                        Console.WriteLine("Left: " + i + " Right: " + j + " Has value: " + SudokuValue[i * 9 + j]);
                        Temp.Insert(j, Convert.ToInt16(SudokuValue[i * 9 + j]));

                    }
                    BoardValue.Insert(i, Temp);

                }
                for (int i = 0; i < 9; i++)
                {
                    var Temp = new List<int>();
                    for (int j = 0; j < 9; j++)
                    {

                        Console.WriteLine("Left: " + i + " Right: " + j + " Has Placement: " + SudokuPlacement[i * 9 + j]);
                        Temp.Insert(j, Convert.ToInt16(SudokuPlacement[i * 9 + j]));

                    }
                    BoardPlacement.Insert(i, Temp);

                }


                var SudoVal = JsonConvert.SerializeObject(BoardValue);
                var SudoPlc = JsonConvert.SerializeObject(BoardPlacement);


                var Sudo = new SudokuTable();
                Sudo.Difficulty = "Normal";
                Sudo.ValueList = SudoVal;
                Sudo.PlacedList = SudoPlc;
                Insert(Sudo);
            }
        }

        public void MakePicross() // Function that makes the data for 7 Picross puzzles
        {
            string PX1 = "1011101011100100111100101100100101011010101011010001111010011100010000101110011010100000110001101110";
            string TR1 = "21301201122021111011211031021110111104302110";
            string LR1 = "13120114012101121011210411021013201120230";
            string PX2 = "1111111111100000000110100001011010000101100000000110000000001010000101101111110110000000011111111111";
            string TR2 = "A011012210111011101110111012210110A";
            string LR2 = "A01101111011110110110111101610110A";
            string PX3 = "0010100000011111000001111101001101011001011111001100111000101111110011101111100100111111110110111000";
            string TR3 = "12041103608036044013011031023";
            string LR3 = "110505102121052031062015108023";
            string PX4 = "0011001100001100110000110011000011001100011111111011111111111111111111110111101111110011110111111110";
            string TR4 = "4060720A0410410A0720604";
            string LR4 = "22022022022080A0A024204408";
            string PX5 = "0000101011000000001100000010000000100001000000010000000110011100111100110111111001111111100001010010";
            string TR5 = "203010301130501140130230211";
            string LR5 = "112020101101021024026080111";
            string PX6 = "0000010000000111110000011111001101011100010111111101010111010111011101011111110100011111110001111100";
            string TR6 = "105020902130A090901105";
            string LR6 = "1050502130170113103310710705";
            string PX7 = "0000010000000111110000011111001101011100010111111101010111010111011101011111110100011111110001111100";
            string TR7 = "105020902130A090901105";
            string LR7 = "1050502130170113103310710705";

            List<string> PXList = new List<string>() { PX1, PX2, PX3, PX4, PX5, PX6, PX7 };
            List<string> LRList = new List<string>() { LR1, LR2, LR3, LR4, LR5, LR6, LR7 };
            List<string> TRList = new List<string>() { TR1, TR2, TR3, TR4, TR5, TR6, TR7 };

            for (int k = 0; k < 7; k++)
            {


                string Picross = PXList[k];
                string Toprow = TRList[k];
                string Leftrow = LRList[k];

                var Board = new List<List<int>>();

                for (int i = 0; i < 10; i++)
                {
                    var Temp = new List<int>();
                    for (int j = 0; j < 10; j++)
                    {

                        Console.WriteLine("Left: " + i + " Right: " + j + " Has value: " + Picross[i * 10 + j]);
                        Temp.Insert(j, Convert.ToInt16(Picross[i * 10 + j]));

                    }
                    Board.Insert(i, Temp);

                }

                var TopList = new List<string>();
                string Topstring = "";

                foreach (Char x in Toprow)
                {

                    if (x == '0')
                    {
                        Topstring = Topstring.Replace("A", "10");
                        TopList.Add(Topstring);
                        Topstring = "";
                    }
                    else
                    {
                        Topstring += x + "\n";
                    }
                }

                var LeftList = new List<string>();
                string Leftstring = "";

                foreach (Char x in Leftrow)
                {

                    if (x == '0')
                    {
                        Leftstring = Leftstring.Replace("A", "10");
                        LeftList.Add(Leftstring);
                        Leftstring = "";
                    }
                    else
                    {
                        Leftstring += x + " ";
                    }
                }

                var Top = JsonConvert.SerializeObject(TopList);
                var Left = JsonConvert.SerializeObject(LeftList);
                var SBoard = JsonConvert.SerializeObject(Board);

                var Pic = new PicrossTable();
                Pic.Top = Top;
                Pic.Left = Left;
                Pic.Gameboard = SBoard;
                Insert(Pic);
            }
        }


        public bool ParseRssFile() // Function that parses an RSS flow, extracts the data from specific nodes and makes an RSStable object which gets inserted
        {

            string XML = "";


            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                client.DownloadFile("https://sla.stage.nwt.infomaker.io/feed/all/", "temp.xml");
                XML = File.ReadAllText("temp.xml");
            }

            //Console.WriteLine("Read File Content: "+XML);

            var Compare = RSSQuery("SELECT * FROM RSS");
            XmlDocument rssXmlDoc = new XmlDocument();

            // Load the RSS file from the RSS URL
            rssXmlDoc.LoadXml(XML.Replace("&", "&amp;"));

            // Parse the Items in the RSS file
            XmlNodeList rssNodes = rssXmlDoc.SelectNodes("rss/channel/item");

            StringBuilder rssContent = new StringBuilder();

            XmlNamespaceManager nmspc = new XmlNamespaceManager(rssXmlDoc.NameTable);
            nmspc.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
              
            // Iterate through the items in the RSS file


            foreach (XmlNode rssNode in rssNodes) // For all articles in the RSS flow.
            {
                var RSS = new RSSTable();
                XmlNode rssSubNode = rssNode.SelectSingleNode("title");
                RSS.Title = rssSubNode != null ? rssSubNode.InnerText : "";

                RSS.Title = RSS.Title.Replace("“", "\"").Replace("”", "\"").Replace("–", "*").Replace("&amp;", "&");


                rssSubNode = rssNode.SelectSingleNode("description");
                RSS.Description = rssSubNode != null ? rssSubNode.InnerText : "";

                RSS.Description = RSS.Description.Replace("“", "\"").Replace("”", "\"").Replace("–", "*").Replace("&amp;", "&");

                rssSubNode = rssNode.SelectSingleNode("pubDate");
                var date = rssSubNode != null ? rssSubNode.InnerText : "";

                while (true)
                {
                    if(Char.IsDigit(date[0]))
                        break;
                    else
                    {
                        date = date.Remove(0, 1);
                    }

                }

                RSS.PubDate = DateTime.Parse(date);
                    

                rssSubNode = rssNode.SelectSingleNode("link");
                RSS.Link = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("source");
                RSS.Source = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("plus");
                var Plus = rssSubNode != null ? rssSubNode.InnerText : "";
                if(Plus == "true") { RSS.Plus = 1; } else { RSS.Plus = 0; }

                rssSubNode = rssNode.SelectSingleNode("newsScore");
                RSS.NewsScore = int.Parse(rssSubNode != null ? rssSubNode.InnerText : "0");

                rssSubNode = rssNode.SelectSingleNode("authors");
                RSS.Author = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("categories");
                RSS.Category = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("tags");
                RSS.Tag = rssSubNode != null ? rssSubNode.InnerText : "";

                RSS.Category = RSS.Category.Replace("“", "\"").Replace("”", "\"").Replace("–", "*").Replace("&amp;", "&");


                rssSubNode = rssNode.SelectSingleNode("content:encoded", nmspc);
                RSS.Content = rssSubNode != null ? rssSubNode.InnerText : "";

                RSS.ImgSource = "http://media2.hitzfm.nu/2016/11/Nyheter_3472x1074.jpg";

                List<int> OrderList = new List<int>();
                List<string> TextList = new List<string>();
                List<string> IMGList = new List<string>();
                List<string> IMGTextList = new List<string>();
                bool IMGSRCset = false;

                XmlDocument xmltest = new XmlDocument();
                xmltest.LoadXml(RSS.Content);
                XmlDocument xmltemp = new XmlDocument();
                xmltemp.LoadXml(xmltest.DocumentElement.InnerXml);
                XmlNodeList fulllist = xmltemp.DocumentElement.ChildNodes;

                foreach (XmlNode Node in fulllist) // Goes though all Nodes containing the body of the article, determines if the nodes is pure text or an images an notes down the order in which they appear in.
                {

                    if (Node.OuterXml.Contains("element"))
                    {
                        if (Node.OuterXml.Contains("paragraph"))
                        {
                            Node.InnerText = Node.InnerText.Replace("“", "\"").Replace("”", "\"").Replace("–", "*").Replace("&amp;", "&");
                            OrderList.Add(0);
                            TextList.Add(Node.InnerText);
                        }
                    }
                    else if (Node.OuterXml.Contains("links"))
                    {
                        if (Node.OuterXml.Contains("x-im/image") && !Node.OuterXml.Contains("x-im/imagegallery"))
                        {
                            OrderList.Add(1);

                            var Links = Node.ChildNodes[0].ChildNodes[0];
                            var Data = Links.ChildNodes[0];;
                            var W = Data.ChildNodes[0];
                            var H = Data.ChildNodes[1];
                            var Txt = Data.ChildNodes[2];
                            var uuid = Node != null ? Node.Attributes["uuid"].Value : "eb65d51b-054d-5ea3-89c5-ec8e9768514c";
                            var width = W != null ? W.InnerText : "200";
                            var length = H != null ? H.InnerText : "300";

                            do
                            {
                                var WTemp = Convert.ToInt32(width) / 2;
                                var HTemp = Convert.ToInt32(length) / 2;
                                width = WTemp.ToString();
                                length = HTemp.ToString();

                            } while (Convert.ToInt32(width) > 2000 && Convert.ToInt32(length) > 2000);
                            var IMGURL = "";

                            if (Node.OuterXml.Contains("x-im/crop") && false)
                            {
                                    
                                IMGURL = "https://imengine.public.nwt.infomaker.io/image.php?uuid=" + uuid + "&function=cropresize&type=preview&source=false&q=75&crop_w=0.99999&crop_h=0.79225&x=1.0E-5&y=1.0E-5&width=600&height=338";                                 
                            }
                            else
                            {
                                IMGURL = "https://imengine.public.nwt.infomaker.io/image.php?uuid=" + uuid + "&function=hardcrop&type=preview&source=false&q=75&width=" + width + "&height=" + length;
                            }
                            IMGList.Add(IMGURL);
                            Txt.InnerText = Txt.InnerText.Replace("“", "\"").Replace("”", "\"").Replace("–", "-").Replace("&amp;", "&");
                            IMGTextList.Add(Txt.InnerText);
                            if (IMGSRCset == false)
                            {
                                RSS.ImgSource = IMGURL;
                                IMGSRCset = true;
                            }                                                              
                        }
                    }
                    else
                    {
                        //;
                    }
                }

                string JSONOrder = JsonConvert.SerializeObject(OrderList);
                string JSONText = JsonConvert.SerializeObject(TextList);
                string JSONIMG = JsonConvert.SerializeObject(IMGList);
                string JSONIMGText = JsonConvert.SerializeObject(IMGTextList);

                Boolean Noinsert = false;

                RSS.Ordning = JSONOrder;
                RSS.Text = JSONText;
                RSS.Images = JSONIMG;
                RSS.Imagetext = JSONIMGText;

                    


                foreach (RSSTable rss in Compare) // Compares the title of the current article with the ones already in the database and if it exists in the database it will no insert it.
                {
                    if (RSS.Title == rss.Title)
                        Noinsert = true;
                }

                if (!Noinsert)
                {
                    Insert(RSS);
                    var NF = new NewsfeedTable();
                    var ID = SQLite3.LastInsertRowid(DB.Handle);
                    NF.Article = ID;
                    NF.NewsScore = RSS.NewsScore;
                    NF.Header = RSS.Title;
                    NF.Image = RSS.ImgSource;
                    NF.Plus = RSS.Plus;
                    NF.Author = RSS.Author;
                    NF.Category = RSS.Category;
                    NF.Tag = RSS.Tag;
                    NF.Ingress = RSS.Description;
                    NF.DatePosted = RSS.PubDate;
                    Insert(NF);
                    
                }
                    
            }

            return true;
        }
    }
}