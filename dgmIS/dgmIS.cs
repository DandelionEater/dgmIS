using dgmIS.DBconnect;
using dgmIS.Utilities;

var username = Util.InputMenuString("Iveskite prisijungimo varda");
var password = Util.InputMenuString("Iveskite slaptazodi", true);


var DB = new DBconnect(username, password);

Console.WriteLine(string.Format("Sveiki {0}!", username == "root" ? "administratoriau" : username));

start:

var options = new List<string>();

switch (DB._access)
{
	case DBconnect.Access.Student:
		options.Add("Perziureti pazymius");
		break;
	case DBconnect.Access.Lecturer:
		options.Add("Ivesti nauja pazymi");
		options.Add("Pakeisti esama pazymi");
		break;
	case DBconnect.Access.Admin:
		options.Add("Studento sukurimas");
		options.Add("Studento salinimas");
		options.Add("Studentu grupiu sukurimas");
		options.Add("Studentu grupiu salinimas");
		options.Add("Studento priskirimas prie grupes");
		options.Add("Destytojo sukurimas");
		options.Add("Destytojo salinimas");
		options.Add("Destytojo priskyrimas prie paskaitos");
		options.Add("Paskaitos sukurimas");
		options.Add("Paskaitos salinimas");
		options.Add("Paskaitos priskyrimas prie grupes");
		break;
}

options.Add("Atsijungti");

var selection = Util.SelectionMenu("Pasirinkite norima operacija", options);

switch (DB._access)
{
	case DBconnect.Access.Student:
		switch (selection)
		{
			case 0:
				Util.displayData(Util.convertToStrList(await DB.listGradesForStudent()), new List<string> { "Studento numeris",
					"Paskaitos numeris", "Destytojo numeris", "Pazymys" });
				break;



			default:
				goto end;
		}

		break;
	case DBconnect.Access.Lecturer:
		switch (selection)
		{
			case 0:
				//DB.createGrade();

				break;
			case 1:
				//DB.updateGrade();

				break;



			default:
				goto end;
		}

		break;
	case DBconnect.Access.Admin:
		switch (selection)
		{
			case 0:
				var fName = Util.InputMenuString("Iveskite studento varda");
				var lName = Util.InputMenuString("Iveskite studento pavarde");
				await DB.createStudent(fName, lName);

				Console.WriteLine(string.Format("Studentas sekmingai sukurtas, jo paskyros prisijungimas yra: \nPrisijungimo vardas: {0}\nSlaptazodis: {1}", fName, lName));
				break;
			case 5:
				fName = Util.InputMenuString("Iveskite destytojo varda");
				lName = Util.InputMenuString("Iveskite destytojo pavarde");
				await DB.createLecturer(fName, lName);

				Console.WriteLine(string.Format("Destytojas sekmingai sukurtas, jo paskyros prisijungimas yra: \nPrisijungimo vardas: {0}\nSlaptazodis: {1}", fName, lName));
				break;

			default:
				goto end;
		}

		break;
}

if (Util.SelectionMenu("Ar norite testi?", new List<string> { "Ne", "Taip" }) == 1)
	goto start;

end:

Console.WriteLine("Program has ended");