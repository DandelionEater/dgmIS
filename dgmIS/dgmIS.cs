using dgmIS.DBconnect;

var username = Console.ReadLine();
var password = Console.ReadLine();

var DB = new DBconnect();

DB.createConnection(username, password);
DB.createLecturer("Igor", "Katin", "EIF");
DB.closeConnection();

