//
/*   Server Program    */

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;

namespace NWTServer
{
    //  
    public class JSONObj //JSON Object used when the database logic has to call on itself.
    {
        public string Type { get; set; }
        public string Operation { get; set; }
        public string JSON { get; set; }
        public int UserID { get; set; }
        public JSONObj(string Type_, string OP_, string JSON_, int UserID_ )
        {
        Type = Type_;
        Operation = OP_;
        JSON = JSON_;
        UserID = UserID_;
        }

        public void Insert(string Type_, string OP_, string JSON_,int UserID_)
        {
            Type = Type_;
            Operation = OP_;
            JSON = JSON_;
            UserID = UserID_;
        }
    }

    public class NWTServer
    {
        public static double VersionNumber = 1.1;
       
        public static string SHA256Hash(string input) //SHA256 Generator.
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string Tokengen() //Token String Generator
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            Random rng = new Random();
            string Token = "";

            for (int i = 0; i < 36; i++)
            {
                int num = rng.Next(0, allowedChars.Length - 1);
                Token += allowedChars[num];
            }
            return Token;
        }

        public static string JSONRecurs(JSONObj JSONRecv, string Type_, string OP_, string JSON_, int UserID_) // Function that prepares a JSON Object to be used when the OPStrat function wishes to call itself.
        {
            JSONRecv.Insert(Type_, OP_, JSON_, UserID_);
            return OPStrat(JSONRecv);
        }



        public static string OPStrat(JSONObj RECV) //Function that takes a JSONObj, and depending on its Type and Operation variables will preform a specific action with the data in the "JSON" string of the JSONObj.
        {
            string Result = "NoOp"; //Default return value.
            Boolean MissionUpdate = false; //Decides if the preformed actions should update the users stat.
            var JSONRECV = new JSONObj("", "", "",-1); //JSON object to used when preparing to call the OPStrat from inside itself.
            Console.WriteLine(RECV.JSON);

            if (RECV.Operation == "Execute") // If Operation is Execute, the database will execute the contents of JSONstring directly.
            {
                database.Execute(RECV.JSON);               
            }
            else
            {
                switch (RECV.Type) //Decides what type of request the App wants to preform. Every Type has a Insert and a Query function for getting the data (Delete exists only for Types that can be removed by a user in the app)
                {
                    case "Handshake": //Checks if the installed apps version matches with the server
                        Result = JsonConvert.SerializeObject(VersionNumber);
                        break;
                    case "User": // Operations related to the User.
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<UserTable>(RECV.JSON);
                            database.Insert(Statement);
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {                         
                            Result = JsonConvert.SerializeObject(database.UserQuery(RECV.JSON));
                            break;
                        }                     
                        else if (RECV.Operation == "Register") //Operation that from input data makes a new user for the App.
                        {
                            var Statement = JsonConvert.DeserializeObject<UserTable>(RECV.JSON);
                            Statement.Password = SHA256Hash(Statement.Password); //Password is stored Hashes without salt.
                            var Recur = JSONRecurs(JSONRECV, "User", "Query", "SELECT * FROM Users WHERE Username = '" + Statement.Username + "'",-1);
                            Statement.MissionString = "Not Calculated"; // MissionString will be calculated on first login.

                            List<string> Categories = new List<string>();
                            List<string> Tags = new List<string>();
                            List<string> Authors = new List<string>();

                            List<List<string>> Taglist = new List<List<string>>();

                            Taglist.Add(Categories);
                            Taglist.Add(Tags);
                            Taglist.Add(Authors);
                            Statement.TaggString = JsonConvert.SerializeObject(Taglist);

                            if (JsonConvert.DeserializeObject<List<UserTable>>(Recur).Count == 0) // If the username already exists in the data base, return an "Error" otherwise insert the user.
                            {                           
                                JSONRecurs(JSONRECV, "User", "Insert", JsonConvert.SerializeObject(Statement),-1);


                                Result = "Operation Successful";
                            }
                            else
                            {
                                Result = "User Already Exists";
                            }
                            break;
                        }
                        else if (RECV.Operation == "ChangePassword") //Updates a users password.
                        {                           
                            var Password = JsonConvert.DeserializeObject<KeyValuePair<UserTable, string>>(RECV.JSON);
                            string Statement = "UPDATE Users SET Password = '" + SHA256Hash(Password.Value)  +"' WHERE ID = " + Password.Key.ID;
                            JSONRecurs(JSONRECV, "User", "Execute", Statement,RECV.UserID);
                            break;
                        }
                        else if (RECV.Operation == "UpdateInfo") // Updates a users personal info.
                        {
                            var Update = JsonConvert.DeserializeObject<UserTable>(RECV.JSON);
                            string Statement = "UPDATE Users SET Name = '"+ Update.Name + "', Email = '" + Update.Email + "', City = '" + Update.City + "', Age = " + Update.Age + " WHERE ID = " + Update.ID;
                            JSONRecurs(JSONRECV, "User", "Execute", Statement, RECV.UserID);
                            break;
                        }
                        else if (RECV.Operation == "UpdateChoices") // Updates a users Taggstring.
                        {
                            
                            var Recur = JSONRecurs(JSONRECV, "User", "Query", "SELECT * FROM Users WHERE ID = " + RECV.UserID, RECV.UserID);
                            var UserQ = JsonConvert.DeserializeObject<List<UserTable>>(Recur);
                            var Update = JsonConvert.DeserializeObject<UserTable>(RECV.JSON);
                            if (UserQ.Count == 1)
                            {
                                if (UserQ.First().TaggString.Length < Update.TaggString.Length)
                                {
                                    database.Stats("SubjectAdded", RECV.UserID);
                                }
                                else if (UserQ.First().TaggString.Length > Update.TaggString.Length)
                                {
                                    database.Stats("SubjectRemoved", RECV.UserID);
                                }
                            }                          
                            string Statement = "UPDATE Users SET TaggString ='"+ Update.TaggString + "' WHERE ID = " + Update.ID;
                            JSONRecurs(JSONRECV, "User", "Execute", Statement, RECV.UserID); 
                            break;
                        }
                        else if (RECV.Operation == "UpdateTutorial") //Updates a users tutorial progress
                        {
                            var Update = JsonConvert.DeserializeObject<UserTable>(RECV.JSON);
                            string Statement = "UPDATE Users SET TutorialProgress ='" + Update.TutorialProgress + "' WHERE ID = " + Update.ID;
                            JSONRecurs(JSONRECV, "User", "Execute", Statement, RECV.UserID);
                            break;
                        }
                        else if (RECV.Operation == "UpdateItems") // Updates a users inventory of shop items or current avatar.
                        {
                            var Recur = JSONRecurs(JSONRECV, "User", "Query", "SELECT * FROM Users WHERE ID = " + RECV.UserID, RECV.UserID);
                            var UserQ = JsonConvert.DeserializeObject<List<UserTable>>(Recur);
                            var Update = JsonConvert.DeserializeObject<UserTable>(RECV.JSON);

                            if (UserQ.Count == 1)
                            {
                                if(Update.Avatar != UserQ.First().Avatar)
                                {
                                    database.Stats("AvatarChanged", RECV.UserID);
                                }
                                else if (Update.Style != UserQ.First().Style)
                                {
                                    database.Stats("StylesChanged", RECV.UserID);
                                }
                                else if (Update.Inventory != UserQ.First().Inventory)
                                {
                                    var Inventory = JsonConvert.DeserializeObject<List<int>>(Update.Inventory);
                                    if ( 20 < Inventory.Last()  && Inventory.Last() < 46)
                                    {
                                        database.Stats("BoughtStyles", RECV.UserID);
                                    }
                                    else
                                    {
                                        database.Stats("BoughtItems", RECV.UserID);
                                    }

                                    
                                }


                            }
                            string Statement = "UPDATE Users SET Inventory ='" + Update.Inventory + "', Avatar = '" + Update.Avatar + "', Style = '" + Update.Style + "' WHERE ID = " + Update.ID;
                            JSONRecurs(JSONRECV, "User", "Execute", Statement, RECV.UserID);
                            break;
                        }
                        else if (RECV.Operation == "Plustoken") // Grants or removes Plustokens from a user.
                        {
                            var User = JsonConvert.DeserializeObject<KeyValuePair<UserTable, int>>(RECV.JSON);
                            if(User.Key.Plustokens + User.Value >= 0)
                            {
                                string Statement = "UPDATE Users SET Plustokens = "+ (User.Key.Plustokens + User.Value) +" WHERE ID = " + User.Key.ID;
                                JSONRecurs(JSONRECV, "User", "Execute", Statement, RECV.UserID);
                                
                                if(User.Value > 0)
                                {
                                    for (int i = 0; i < User.Value; i++)
                                    {
                                        database.Stats("TokensCollected", RECV.UserID);
                                    }
                                }
                                else if (User.Value < 0)
                                {
                                    for (int i = 0; i > User.Value; i--)
                                    {
                                        database.Stats("CoinsSpent", RECV.UserID);
                                    }
                                }
                                Result = JsonConvert.SerializeObject(true);
                            }
                            else
                            {
                                if(User.Key.Plustokens < 0)
                                {
                                    string Statement = "UPDATE Users SET Plustokens = '0' WHERE ID = " + User.Key.ID;
                                    JSONRecurs(JSONRECV, "User", "Execute", Statement, RECV.UserID);
                                }
                                Result = JsonConvert.SerializeObject(false);
                            }
                            break;
                        }
                        else if (RECV.Operation == "Mission") // Evalutes the Users progress on their missions.
                        {
                            database.Mission("Evaluate", RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        break;

                    case "Comments": // Operations realated to comments.
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<CommentTable>(RECV.JSON);
                            database.Insert(Statement);
                            database.Stats("CommentsPosted", RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.CommentQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "Point") // Modifies the point value of a comment.
                        {
                            var Comment = JsonConvert.DeserializeObject<KeyValuePair<CommentTable, int>>(RECV.JSON);
                            
                                string Statement = "UPDATE Comments SET Point = " + (Comment.Key.Point + Comment.Value) + " WHERE ID = " + Comment.Key.ID;
                                JSONRecurs(JSONRECV, "User", "Execute", Statement, RECV.UserID);
                                Result = JsonConvert.SerializeObject(true);
                                break;
                        }
                        break;

                    case "Token": // Operations related to logins and UserTokens
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<TokenTable>(RECV.JSON);
                            database.Insert(Statement);
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.TokenQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "Delete")
                        {
                            var Statement = JsonConvert.DeserializeObject<TokenTable>(RECV.JSON);
                            database.Delete(Statement);
                            break;
                        }
                        else if (RECV.Operation == "Login")// When the user logs in to the app.
                        {
                            var Statement = JsonConvert.DeserializeObject<UserTable>(RECV.JSON);
                            Statement.Password = SHA256Hash(Statement.Password);
                            var UpperPassword = Statement.Password.ToUpper();
                            var Recur = JSONRecurs(JSONRECV, "User", "Query", "SELECT * FROM Users WHERE (Username = '" + Statement.Username + "' OR Email = '" + Statement.Email + "') AND (Password = '" + Statement.Password + "' OR Password = '"+ UpperPassword +"')", RECV.UserID);
                            var UserQ = JsonConvert.DeserializeObject<List<UserTable>>(Recur);
                            if (UserQ.Count == 1) //Compares the inputed password value after it is Hashed, if it matches a Token will be generated.
                            {
                                var Tokenstring = Tokengen();

                                Recur =  JSONRecurs(JSONRECV, "Token", "Query", "SELECT * FROM Tokens WHERE User = '" + UserQ.First().ID + "'", RECV.UserID);
                               
                                if (JsonConvert.DeserializeObject<List<TokenTable>>(Recur).Count > 0) // Removes tokens from the same user if they were already logged in on another device.
                                {
                                    JSONRecurs(JSONRECV, "Token", "Delete", JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<TokenTable>>(Recur).First()), RECV.UserID);                                    
                                }

                                var Token = new TokenTable();
                                Token.Token = Tokenstring;
                                Token.User = UserQ.First().ID;
                                Token.LastUse = DateTime.Now.ToString();
                                Console.WriteLine("Tokenstring: " + Token.Token);
                                JSONRecurs(JSONRECV, "Token", "Insert", JsonConvert.SerializeObject(Token), RECV.UserID);
                                var Returntoken = JSONRecurs(JSONRECV, "Token", "Query", "SELECT * FROM Tokens WHERE Token = '" + Tokenstring + "'", RECV.UserID);
                                Result = Returntoken;

                                if (UserQ.First().MissionString == "Not Calculated") //Calculates Stats and mission string if it is the users first time login in.
                                {                                 
                                    database.Mission("Reset", UserQ.First().ID);
                                    var Stats = new StatsTable();
                                    Stats.User = UserQ.First().ID;
                                    Stats.Logins = 0;
                                    Stats.UseTime = 0;
                                    Stats.ArticlesRead = 0;
                                    Stats.PlusArticlesUnlocked = 0;
                                    Stats.InsandareSubmitted = 0;
                                    Stats.InsandareRead = 0;
                                    Stats.GameFinished = 0;
                                    Stats.QuestionSubmitted = 0;
                                    Stats.QuestionAnswered = 0;
                                    Stats.VoteQuestionSubmitted = 0;
                                    Stats.VoteSubmitted = 0;
                                    Stats.CommentsPosted = 0;
                                    Stats.TokensCollected = 0;
                                    Stats.StylesChanged = 0;
                                    Stats.AvatarChanged = 0;
                                    Stats.BoughtItems = 0;
                                    Stats.BoughtStyles = 0;
                                    Stats.CoinsSpent = 0;
                                    Stats.ArticlesClicked = 0;
                                    Stats.SubjectClicked = 0;
                                    Stats.SubjectAdded = 0;
                                    Stats.SubjectRemoved = 0;
                                    Stats.GenericStat1 = 0;
                                    Stats.GenericStat2 = 0;
                                    Stats.GenericStat3 = 0;
                                    Stats.GenericStat4 = 0;
                                    Stats.GenericStat5 = 0;
                                    JSONRecurs(JSONRECV, "Stats", "Insert", JsonConvert.SerializeObject(Stats), UserQ.First().ID);
                                }
                                database.Stats("Logins", UserQ.First().ID);



                                if (UserQ.First().DailyLogin == 0) //Updates the Users login streak.
                                {
                                    int Streak = UserQ.First().LoginStreak+1;
                                    

                                    int Tokens = Streak + UserQ.First().Plustokens;

                                    if(UserQ.First().LoginStreak == 7)
                                    {
                                        Streak = 0;
                                    }

                                    Recur = "UPDATE Users SET DailyLogin = '" + 1 + "', LoginStreak = '" + Streak + "', Plustokens = '" + Tokens + "' WHERE ID = " + UserQ.First().ID;
                                    JSONRecurs(JSONRECV, "User", "Execute", Recur, RECV.UserID);
                                    
                                }
                            }
                            else
                            {
                                Result = null;
                            }
                            break;
                        }
                        else if (RECV.Operation == "Logout") // When the User Logs out of the app
                        {
                            var Statement = JsonConvert.DeserializeObject<TokenTable>(RECV.JSON);
                            
                            if (JsonConvert.DeserializeObject<Boolean>(JSONRecurs(JSONRECV, "Token", "TokenCheck", JsonConvert.SerializeObject(Statement), RECV.UserID)))
                            {
                                JSONRecurs(JSONRECV, "Token", "Delete", JsonConvert.SerializeObject(Statement), RECV.UserID);
                            }
                            break;
                        }
                        else if (RECV.Operation == "TokenCheck") //Checks if a sent in token matches on in the database
                        {
                            var Statement = JsonConvert.DeserializeObject<TokenTable>(RECV.JSON);
                            Console.WriteLine("Encrypted sent in token: " + Statement.Token);
                            var ServerToken = JsonConvert.DeserializeObject<List<TokenTable>>(JSONRecurs(JSONRECV, "Token", "Query", "SELECT * FROM Tokens WHERE User = '" + Statement.User + "'", RECV.UserID));
                            Console.WriteLine("Servertoken: " + ServerToken);
                            Console.WriteLine("Sent in User ID: " + Statement.User);
                            if(ServerToken.Any()) { 
                                var HashToken = SHA256Hash(ServerToken.First().Token + Statement.User);
                                Console.WriteLine("Encrypted Servertoken: " + HashToken);
                                if (HashToken == Statement.Token)
                                {
                                    
                                    JSONRecurs(JSONRECV, "Token", "Execute", "UPDATE Tokens SET LastUse = '" + DateTime.Now.ToString()  +"' WHERE Token = '" + ServerToken.First().Token + "'", RECV.UserID);
                                    Result = JsonConvert.SerializeObject(true);
                                }
                                else
                                {
                                    Result = JsonConvert.SerializeObject(false);
                                }
                            }
                            else
                            {
                                Result = JsonConvert.SerializeObject(false);
                            }
                            JSONRecurs(JSONRECV, "Token", "TokenCleanup", "", RECV.UserID);
                            break;
                        }
                        else if (RECV.Operation == "TokenCleanup") //Removes tokens that are 1 hour old.
                        {
                            var ServerToken = JsonConvert.DeserializeObject<List<TokenTable>>(JSONRecurs(JSONRECV, "Token", "Query", "SELECT * FROM Tokens", RECV.UserID));
                            if (ServerToken.Any())
                            {
                                foreach(TokenTable T in ServerToken)
                                {
                                    if(DateTime.Parse(T.LastUse).Add(new TimeSpan(1,0,0)) < DateTime.Now)
                                    {
                                        JSONRecurs(JSONRECV, "Token", "Delete", JsonConvert.SerializeObject(T), RECV.UserID);
                                    }
                                }
                            }
                        }
                        break;

                    case "RSS": //Operations to fetch the raw data of an article  (A RSSTable)
                        if (RECV.Operation == "Insert")
                        {
                            //Not used
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {                          
                            database.Stats("ArticlesClicked", RECV.UserID);
                            Result = JsonConvert.SerializeObject(database.RSSQuery(RECV.JSON));
                            break;
                        }
                        break;

                    case "Plus": //Operationer related to Plus articles.
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<PlusRSSTable>(RECV.JSON);
                            database.Insert(Statement);
                            database.Stats("PlusArticlesUnlocked", RECV.UserID);
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.PlusQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "PlusCheck") // Checks if a user has access to a plus article or not.
                        {
                            var Statement = JsonConvert.DeserializeObject<PlusRSSTable>(RECV.JSON);
                            var Query = JsonConvert.DeserializeObject<List<PlusRSSTable>>(JSONRecurs(JSONRECV,"Plus", "Query", "SELECT * FROM Plus WHERE Article = " + Statement.Article + " AND User = " + Statement.User, RECV.UserID));
                            if (Query.Any())
                            {
                                Result = JsonConvert.SerializeObject(true);
                            }
                            else
                            {
                                Result = JsonConvert.SerializeObject(false);
                            }
                            break;
                        }
                        break;

                    case "UserRSS": //User submitted article operations
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<UserRSSTable>(RECV.JSON);
                            database.Insert(Statement);
                            database.Stats("InsandareSubmitted", RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.UserRSSQuery(RECV.JSON));
                            break;
                        }
                        break;

                    case "Sudoku": //Sudoku related Operations.
                        if (RECV.Operation == "Insert")
                        {
                            //Not used
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            var Tid = DateTime.Now.DayOfWeek;
                            Result = JsonConvert.SerializeObject(database.SudokuQuery(RECV.JSON));
                            break;
                        }
                        break;

                    case "Stats": //User stat related Operations
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<StatsTable>(RECV.JSON);
                            database.Insert(Statement);
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.StatsQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "Update")
                        {
                            
                            database.Stats(RECV.JSON, RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        break;

                    case "Quiz": //Quiz Related Operations
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<QuizTable>(RECV.JSON);
                            database.Insert(Statement);
                            database.Stats("QuestionSubmitted", RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.QuizQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "GetQuestion")
                        {
                            
                            break;
                        }
                        break;

                    case "Favorite": //User Favorite Related Operations
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<FavoritesTable>(RECV.JSON);
                            database.Insert(Statement);
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.FavoriteQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "Delete")
                        {
                            var Statement = JsonConvert.DeserializeObject<FavoritesTable>(RECV.JSON);
                            database.Delete(Statement);
                            break;
                        }
                        break;

                    case "History": //UserHistory Related Operations
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<HistoryTable>(RECV.JSON);

                            database.Insert(Statement);
                            database.Stats("ArticlesRead", RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.HistoryQuery(RECV.JSON));
                            break;
                        }                        
                        break;

                    case "Newsfeed": // Newsfeed Related Operation
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<NewsfeedTable>(RECV.JSON);
                            database.Insert(Statement);
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            if (RECV.JSON.Contains("Tag LIKE") && RECV.JSON.Contains(" - 20"))
                            {
                                database.Stats("SubjectClicked", RECV.UserID);
                            }


                            
                            Result = JsonConvert.SerializeObject(database.NewsfeedQuery(RECV.JSON));
                            break;
                        }
                        break;

                    case "VoteQuestion": // Timed Poll related Operations.
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<VoteQuestionTable>(RECV.JSON);
                            database.Insert(Statement);
                            database.Stats("VoteQuestionSubmitted", RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.VoteQuestionQuery(RECV.JSON));
                            break;
                        }
                        break;

                    case "Vote": // Timed Poll vote data realted Operations
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<VoteTable>(RECV.JSON);
                            database.Insert(Statement);
                            database.Stats("VoteSubmitted", RECV.UserID);
                            MissionUpdate = true;
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.VoteQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "Purge")
                        {
                            var Statement = JsonConvert.DeserializeObject<int>(RECV.JSON);
                            JSONRecurs(JSONRECV, "User", "Execute", "DELETE FROM Votes WHERE Question = " + Statement + "'", RECV.UserID);
                            break;
                        }
                        break;

                    case "Picross": // Picross related operations
                        if (RECV.Operation == "Insert")
                        {
                            //Not Used
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.PicrossQuery(RECV.JSON));
                            break;
                        }
                        break;
                    case "Reaction": //Reaction related operations
                        if (RECV.Operation == "Insert")
                        {
                            var Statement = JsonConvert.DeserializeObject<ReactionTable>(RECV.JSON);
                            database.Insert(Statement);
                            var Recur = JSONRecurs(JSONRECV, "Newsfeed", "Query", "SELECT * FROM Newsfeed WHERE ID = "+Statement.Article, RECV.UserID);
                            var NFQ = JsonConvert.DeserializeObject<List<NewsfeedTable>>(Recur).First();
                            var ReactionList = JsonConvert.DeserializeObject<List<ReactionTable>>(NFQ.ReactionSum);
                            ReactionList.Add(Statement);
                            var RLS = JsonConvert.SerializeObject(ReactionList);
                            string RecState = "UPDATE Newsfeed SET ReactionSum ='" + RLS + "' WHERE ID = " + NFQ.ID;
                            JSONRecurs(JSONRECV, "User", "Execute", RecState, RECV.UserID);
                            break;
                        }
                        else if (RECV.Operation == "Query")
                        {
                            Result = JsonConvert.SerializeObject(database.ReactionQuery(RECV.JSON));
                            break;
                        }
                        else if (RECV.Operation == "Delete")
                        {
                            var Statement = JsonConvert.DeserializeObject<ReactionTable>(RECV.JSON);
                            database.Delete(Statement);
                            break;
                        }
                        break;
                }
            }

            if(MissionUpdate) //If a operation updates the missions string, we return it to the client.
            {
                Result = database.GetMissionString(RECV.UserID);
            }
            return Result;
        }

        public static DBHelper database;

        public static void Main() // Main Listening loop.
        {
            


            Console.Clear();
            if (database == null)
            {
                database = new DBHelper("ServerDB.db3"); // Path to the location of the sqllite3 database.
                database.MakeSudoku();
                database.MakePicross();
                
            }
            database.ParseRssFile();
            IPAddress ipAd;
            // use local m/c IP address, and 
            // use the same in the client

            ipAd = IPAddress.Parse("192.168.1.164"); // Adress the server runs on

            
            /* Initializes the Listener */
            TcpListener myList = new TcpListener(ipAd, 1518);
            
            try
            {

                /* Start Listeneting at the specified port */
                myList.Start();

                Console.WriteLine("The server is running at port 1518...");
                Console.WriteLine("The local End point is  :" +
                                  myList.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");


                while (true) 
                {
                    string Message = "";
                    string Result = "";

                    Socket s = myList.AcceptSocket(); // Receives request from clients
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                    byte[] b1 = new byte[10000];
                    int k = s.Receive(b1); // Recieves Message and runs data through the Operation Strategy
                    Console.WriteLine("Recieved...");
                    for (int i = 0; i < k; i++)
                        Message += Convert.ToChar(b1[i]);
                    var RECV = JsonConvert.DeserializeObject<JSONObj>(Message); 
                    Result = OPStrat(RECV);
                    Console.WriteLine(Result);

                    var JSONSendBack = JsonConvert.SerializeObject(new JSONObj(RECV.Type, RECV.Operation, Result, RECV.UserID)); // Sends the data obtained by the database back to the client.
                    Encoding asen = Encoding.Default;
                    var msg = asen.GetBytes(JSONSendBack);
                    s.Send(asen.GetBytes(msg.Length.ToString()));

                    Message = ""; //Waits for accknowledgement that the data has been recieved and the closes socket.
                    byte[] b2 = new byte[100];
                    k = s.Receive(b2);
                    for (int i = 0; i < k; i++)
                        Message += Convert.ToChar(b2[i]);
                    
                    
                    if (Message == "OK")
                    {
                        s.Send(msg, msg.Length, SocketFlags.None);
                        Console.WriteLine("\nSent Acknowledgement");
                    }
                    
                    s.Close();
                }
            }
            catch (Exception e)
            {
                
                myList.Stop();
                Console.WriteLine("Test..... "+e+"   " + e.StackTrace);
                
                System.Environment.Exit(1);
            }
        }

    }
}
