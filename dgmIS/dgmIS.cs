using dgmIS.DBconnect;
using System.Collections.Generic;

var username = Console.ReadLine();
var password = Console.ReadLine();

var DB = new DBconnect();

DB.createConnection(username, password)
	.createSlogin();
DB.closeConnection();

