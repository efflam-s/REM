using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Wiring
{
    static class SchemReader
    {
        public class SyntaxException : Exception
        {
            public SyntaxException(string message = "") : base(message) { }
            public SyntaxException(int i, char c, object state) : base("character n°" + i + " : " + c + ", at state : " + state.ToString()) { }
        }
        public class StructureException : Exception
        {
            public StructureException(string message = "") : base(message) { }
            public StructureException(int i, string w, object state) : base("word n°" + i + " : " + w + ", at state : " + state.ToString()) { }
        }
        public class MissingFieldException : Exception
        {
            public MissingFieldException(string message = "") : base(message) { }
            public MissingFieldException(string field, string parent) : base("missing field " + field + " in " + parent) { }
        }
        public class InvalidValueException : Exception
        {
            public InvalidValueException(string message = "") : base(message) { }
            public InvalidValueException(string field, object value, string parent) : base("invalid field " + field + " value : " + value.ToString() + " in " + parent) { }
        }
        public class UndefinedWordWarning : Exception
        {
            public UndefinedWordWarning(string message = "") : base(message) { }
            public UndefinedWordWarning(string word, string parent) : base("undefined word : " + word + " in " + parent) { }
        }
        public class InvalidTypeWarning : Exception
        {
            public InvalidTypeWarning(string message = "") : base(message) { }
            public InvalidTypeWarning(string expected, object given, string parent) : base("expected type " + expected + " but got " + given.ToString() + " in " + parent) { }
        }
        public class InvalidListSizeWarning : Exception
        {
            public InvalidListSizeWarning(string message = "") : base(message) { }
            public InvalidListSizeWarning(int expected, int given, string parent) : base("expected size " + expected + " but got " + given.ToString() + " in " + parent) { }
        }

        public static Schematic Read(string path, bool ignoreWarnings = false)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            Schematic newSchem = Read(fs, ignoreWarnings);
            fs.Close();
            return newSchem;
        }
        public static Schematic Read(FileStream fs, bool ignoreWarnings = false)
        {
            string path = fs.Name;
            string[] pathStrings = path.Split('/', '\\');
            string folderPath = string.Join("\\", pathStrings, 0, pathStrings.Length - 1);
            
            string s;
            using (StreamReader reader = new StreamReader(fs))
            {
                s = reader.ReadToEnd();
            }
            //string s = File.ReadAllText(string.Join("\\", pathStrings));

            string[] words = WordParser(s);

            int i = 0;
            Dictionary<string, object> tree = CreateTree(words, ref i);

            // on laisse le choix de mettre "Schematic : { Name : ... }" ou directement "Name : ..."
            if (tree.ContainsKey("Schematic") && tree["Schematic"] is Dictionary<string, object> schematic)
            {
                if (!ignoreWarnings)
                    checkSchem(schematic);
                return TreeToSchem(schematic, folderPath, ignoreWarnings);
            }
            else
            {
                if (!ignoreWarnings)
                    checkSchem(tree);
                return TreeToSchem(tree, folderPath, ignoreWarnings);
            }
        }

        private enum WordParserState
        {
            out_,
            comment,
            aphanum,
            num,
            str
            // autres : {}[]:, (un seul caractère)
        }
        /// <summary>
        /// Permet de séparer la chaîne d'entrée en différents mots : nombres, mot-clés, chaînes, opérateurs...
        /// </summary>
        private static string[] WordParser(string s, bool print=false)
        {
            List<string> words = new List<string>();
            StringBuilder currentWord = new StringBuilder();
            WordParserState state = WordParserState.out_;
            for (int i = 0; i < s.Length; i++)
            {
                switch (state)
                {
                    case WordParserState.out_:
                        if (s[i] == '#')
                        {
                            state = WordParserState.comment;
                        }
                        else if (('a' <= s[i] && s[i] <= 'z') || ('A' <= s[i] && s[i] <= 'Z') || s[i] == '_')
                        {
                            state = WordParserState.aphanum;
                            currentWord = new StringBuilder(new string(s[i], 1));
                        }
                        else if (('0' <= s[i] && s[i] <= '9') || s[i] == '-' || s[i] == '+')
                        {
                            state = WordParserState.num;
                            currentWord = new StringBuilder(new string(s[i], 1));
                        }
                        else if (s[i] == '"')
                        {
                            state = WordParserState.str;
                            currentWord = new StringBuilder(new string(s[i], 1));
                        }
                        else if (s[i] == '{' || s[i] == '}' || s[i] == '[' || s[i] == ']' || s[i] == ':' || s[i] == ',')
                        {
                            words.Add(new string(s[i], 1));
                        }
                        else if (s[i] == '\t' || s[i] == ' ' || s[i] == '\r' || s[i] == '\n')
                        {
                        }
                        else
                        {
                            throw new SyntaxException(i, s[i], state);
                        }
                        break;
                    case WordParserState.comment:
                        if (s[i] == '\n')
                        {
                            state = WordParserState.out_;
                        }
                        break;
                    case WordParserState.aphanum:
                        if (('a' <= s[i] && s[i] <= 'z') || ('A' <= s[i] && s[i] <= 'Z') || s[i] == '_' || '0' <= s[i] && s[i] <= '9')
                        {
                            currentWord.Append(s[i]);
                        }
                        else if (s[i] == '#')
                        {
                            state = WordParserState.comment;
                            words.Add(currentWord.ToString());
                        }
                        else if (s[i] == '"')
                        {
                            throw new SyntaxException(i, s[i], state);
                        }
                        else if (s[i] == '{' || s[i] == '}' || s[i] == '[' || s[i] == ']' || s[i] == ':' || s[i] == ',')
                        {
                            state = WordParserState.out_;
                            words.Add(currentWord.ToString());
                            words.Add(new string(s[i], 1));
                        }
                        else if (s[i] == '\t' || s[i] == ' ' || s[i] == '\n' || s[i] == '\r')
                        {
                            state = WordParserState.out_;
                            words.Add(currentWord.ToString());
                        }
                        else
                        {
                            throw new SyntaxException(i, s[i], state);
                        }
                        break;
                    case WordParserState.num:
                        if (s[i] == '#')
                        {
                            state = WordParserState.comment;
                            words.Add(currentWord.ToString());
                        }
                        else if (('0' <= s[i] && s[i] <= '9'))
                        {
                            currentWord.Append(s[i]);
                        }
                        else if (s[i] == '\t' || s[i] == ' ' || s[i] == '\n' || s[i] == '\r')
                        {
                            state = WordParserState.out_;
                            words.Add(currentWord.ToString());
                        }
                        else if (s[i] == '{' || s[i] == '}' || s[i] == '[' || s[i] == ']' || s[i] == ':' || s[i] == ',')
                        {
                            state = WordParserState.out_;
                            words.Add(currentWord.ToString());
                            words.Add(new string(s[i], 1));
                        }
                        else
                        {
                            throw new SyntaxException(i, s[i], state);
                        }
                        break;
                    case WordParserState.str:
                        // TODO : \
                        if (s[i] == '"')
                        {
                            state = WordParserState.out_;
                            words.Add(currentWord.ToString());
                        }
                        else
                        {
                            currentWord.Append(s[i]);
                        }
                        break;
                }
            }
            if (print)
            {
                foreach (string w in words)
                    Console.Write("(" + w + "), ");
                Console.WriteLine("");
            }
            return words.ToArray();
        }

        private enum CreateTreeState
        {
            name,
            colon,
            value,
            comma
        }
        /// <summary>
        /// Permet d'organiser une liste de mots sous forme d'un arbre de dictionnaires et de listes
        /// </summary>
        private static Dictionary<string, object> CreateTree(string[] w, ref int start, bool print = false)
        {
            Dictionary<string, object> Tree = new Dictionary<string, object>();
            CreateTreeState state = CreateTreeState.name;
            string currentName = "";
            int i;
            for (i = start; i < w.Length && !(state == CreateTreeState.comma && w[i][0] == '}'); i++)
            {
                switch (state)
                {
                    case CreateTreeState.name:
                        if (('a' <= w[i][0] && w[i][0] <= 'z') || ('A' <= w[i][0] && w[i][0] <= 'Z') || w[i][0] == '_')
                        {
                            state = CreateTreeState.colon;
                            currentName = w[i];
                        }
                        else
                        {
                            throw new StructureException(i, w[i], state);
                        }
                        break;
                    case CreateTreeState.colon:
                        if (w[i] == ":")
                        {
                            state = CreateTreeState.value;
                        } else
                        {
                            throw new StructureException(i, w[i], state);
                        }
                        break;
                    case CreateTreeState.value:
                        state = CreateTreeState.comma;
                        if (w[i][0] == '"')
                            Tree.Add(currentName, w[i].Substring(1));
                        else if (('0' <= w[i][0] && w[i][0] <= '9') || w[i][0] == '+' || w[i][0] == '-')
                            Tree.Add(currentName, int.Parse(w[i]));
                        else if (w[i] == "true")
                            Tree.Add(currentName, true);
                        else if (w[i] == "false")
                            Tree.Add(currentName, false);
                        else if (w[i] == "{")
                        {
                            i++;
                            Tree.Add(currentName, CreateTree(w, ref i));
                        }
                        else if (w[i] == "[")
                        {
                            i++;
                            Tree.Add(currentName, CreateList(w, ref i));
                        }
                        else
                        {
                            throw new StructureException(i, w[i], state);
                        }
                        break;
                    case CreateTreeState.comma:
                        if (w[i][0] == ',')
                        {
                            state = CreateTreeState.name;
                        }
                        else
                        {
                            throw new StructureException(i, w[i], state);
                        }
                        break;
                }
            }
            if (state != CreateTreeState.comma)
            {
                throw new StructureException(w.Length-1, w[w.Length-1], state);
            }
            if (print)
            {
                foreach (string str in Tree.Keys)
                {
                    Console.Write("(" + str + " | " + Tree[str] + "), ");
                }
                Console.WriteLine("");
            }
            start=i;
            return Tree;
        }
        private static List<object> CreateList(string[] w, ref int start)
        {
            List<object> list = new List<object>();
            int i;
            for (i = start; i < w.Length && w[i] != "]"; i++)
            {
                
                if (w[i][0] == '"')
                    list.Add(w[i].Substring(1));
                else if (('0' <= w[i][0] && w[i][0] <= '9') || w[i][0] == '+' || w[i][0] == '-')
                    list.Add(int.Parse(w[i]));
                else if (w[i] == "true")
                    list.Add(true);
                else if (w[i] == "false")
                    list.Add(false);
                else if (w[i] == "{")
                {
                    i++;
                    list.Add(CreateTree(w, ref i));
                }
                else if (w[i] == "[")
                {
                    i++;
                    list.Add(CreateList(w, ref i));
                }
                else
                    throw new StructureException(i, w[i], "list");
            }
            if (i >= w.Length)
            {
                throw new StructureException(i, "", "list");
            }
            start = i;
            return list;
        }

        /// <summary>
        /// Vérifie que tout les mot-clés de l'arbre existe et qu'ils sont du bon type
        /// </summary>
        /// <param name="tree"></param>
        private static void checkSchem(Dictionary<string, object> tree)
        {
            foreach (string key in tree.Keys)
            {
                if (key == "Name")
                {
                    if (!(tree["Name"] is string))
                        throw new InvalidTypeWarning("string", tree["Name"], "Schematic");
                }
                else if (key == "Components")
                {
                    if (tree["Components"] is List<object> comps)
                    {
                        // On vérifie que c'est une liste de composants
                        foreach (object obj in comps)
                        {
                            if (obj is Dictionary<string, object> comp)
                                checkComp(comp);
                            else
                                throw new InvalidTypeWarning("Dictionary<string, object>", obj, "Schematic");
                        }
                    }
                    else
                        throw new InvalidTypeWarning("List<object>", tree["Components"], "Schematic");
                }
                else
                    throw new UndefinedWordWarning(key, "Schematic");
            }
        }
        private static void checkComp(Dictionary<string, object> tree)
        {
            foreach (string key in tree.Keys)
            {
                if (key == "Type")
                {
                    if (!(tree["Type"] is string))
                        throw new InvalidTypeWarning("string", tree["Type"], "Component");
                }
                else if (key == "Position")
                {
                    if (tree["Position"] is List<object> position)
                    {
                        if (position.Count == 2)
                        {
                            if (!(position[0] is int))
                                throw new InvalidTypeWarning("int", position[0], "Position");
                            if (!(position[1] is int))
                                throw new InvalidTypeWarning("int", position[1], "Position");
                        }
                        else
                            throw new InvalidListSizeWarning(2, position.Count, "Postion");
                    }
                    else
                        throw new InvalidTypeWarning("List", tree["Position"], "Component");
                }
                else if (key == "Wires")
                {
                    if (tree["Wires"] is List<object> wires)
                    {
                        if (tree.ContainsKey("Type") && tree["Type"] is string type)
                        {
                            if ((type == "Input" || type == "Output") && wires.Count != 1)
                                throw new InvalidListSizeWarning(1, wires.Count, "Wires of "+type);
                            if ((type == "Not" || type == "Diode") && wires.Count != 2)
                                throw new InvalidListSizeWarning(2, wires.Count, "Wires of "+type);
                        }
                        foreach (object obj in wires)
                            if (!(obj is int))
                                throw new InvalidTypeWarning("int", obj, "Wires");
                    }
                    else
                        throw new InvalidTypeWarning("List<object>", tree["Wires"], "Component");
                }
                else if (key == "Data")
                {
                    if (tree["Data"] is Dictionary<string, object> data)
                    {
                        checkData(data);
                    }
                    else
                        throw new InvalidTypeWarning("Dictionary<string, object>", tree["Data"], "Component");
                }
                else
                    throw new UndefinedWordWarning(key, "Component");
            }
        }
        private static void checkData(Dictionary<string, object> tree)
        {
            foreach (string key in tree.Keys)
            {
                if (key == "value")
                {
                    if (!(tree["value"] is bool))
                        throw new InvalidTypeWarning("bool", tree["value"], "Data");
                }
                else if (key == "delay")
                {
                    if (!(tree["delay"] is int))
                        throw new InvalidTypeWarning("int", tree["delay"], "Data");
                }
                else if (key == "path")
                {
                    if (!(tree["path"] is string))
                        throw new InvalidTypeWarning("string", tree["path"], "Data");
                }
                else if (key == "schematic")
                {
                    if (tree["schematic"] is Dictionary<string, object> schematic)
                    {
                        checkSchem(schematic);
                    }
                    else
                        throw new InvalidTypeWarning("Dictionary<string, object>", tree["schematic"], "Data");
                }
                else
                    throw new UndefinedWordWarning(key, "Data");
            }
        }

        /// <summary>
        /// Transforme un arbre de dictionnaires et listes en un schematic
        /// </summary>
        private static Schematic TreeToSchem(Dictionary<string, object> tree, string folderPath, bool ignoreWarnings = false)
        {
            if (tree.ContainsKey("Name") && tree["Name"] is string Name)
            {
                Schematic schem = new Schematic(Name);
                if (tree.ContainsKey("Components") && tree["Components"] is List<object> components)
                {
                    foreach(object c in components)
                    {
                        if (c is Dictionary<string, object> comp)
                        {
                            schem.AddComponent(TreeToComp(comp, folderPath, schem.wires, ignoreWarnings), false);
                        }
                    }
                }
                return schem;
            }
            else
                throw new MissingFieldException("Name", "Schematic");
        }
        private static Component TreeToComp(Dictionary<string, object> tree, string folderPath, List<Wire> SchemWires = null, bool ignoreWarnings = false)
        {
            if (SchemWires == null)
                SchemWires = new List<Wire>();

            if (tree.ContainsKey("Type") && tree["Type"] is string type
                && tree.ContainsKey("Position") && tree["Position"] is List<object> position && position.Count == 2
                && position[0] is int X && position[1] is int Y
                && tree.ContainsKey("Wires") && tree["Wires"] is List<object> wires
                && wires.TrueForAll(w => w is int))
            {
                List<int> wiresId = new List<int>();
                foreach(object w in wires)
                {
                    if (w is int i)
                    {
                        while (i >= SchemWires.Count)
                            SchemWires.Add(new Wire());
                        wiresId.Add(i);
                    }
                }
                Component comp;
                switch (type)
                {
                    case "Input":
                        if (wiresId.Count < 1) throw new InvalidValueException("Wires", wiresId, "Component");
                        Input inp = new Input(SchemWires[wiresId[0]], new Vector2(X, Y));
                        if (tree.ContainsKey("Data") && tree["Data"] is Dictionary<string, object> data0)
                            if (data0.ContainsKey("value") && data0["value"] is bool value)
                                inp.changeValue(value);
                        comp = inp;
                        break;
                    case "Output":
                        if (wiresId.Count < 1) throw new InvalidValueException("Wires", wiresId, "Component");
                        comp = new Output(SchemWires[wiresId[0]], new Vector2(X, Y));
                        break;
                    case "Not":
                        if (wiresId.Count < 2) throw new InvalidValueException("Wires", wiresId, "Component");
                        comp = new Not(SchemWires[wiresId[0]], SchemWires[wiresId[1]], new Vector2(X, Y));
                        break;
                    case "Diode":
                        if (wiresId.Count < 2) throw new InvalidValueException("Wires", wiresId, "Component");
                        Diode dio = new Diode(SchemWires[wiresId[0]], SchemWires[wiresId[1]], new Vector2(X, Y));
                        if (tree.ContainsKey("Data") && tree["Data"] is Dictionary<string, object> data1)
                            if (data1.ContainsKey("delay") && data1["delay"] is int delay)
                                for (int i=0; i<delay; i++)
                                    dio.changeDelay();
                        comp = dio;
                        break;
                    case "BlackBox":
                        // Création d'un nouveau Schematic, et lecture depuis un path ou un attribut schematic
                        Schematic newSchem = new Schematic("undefined");
                        if (tree.ContainsKey("Data") && tree["Data"] is Dictionary<string, object> data2)
                            if (data2.ContainsKey("schematic") && data2["schematic"] is Dictionary<string, object> schematic)
                                newSchem = TreeToSchem(schematic, folderPath, ignoreWarnings);
                            else if (data2.ContainsKey("path") && data2["path"] is string path)
                                newSchem = Read(folderPath + '\\' + path, ignoreWarnings);

                        comp = new BlackBox(newSchem, new Vector2(X, Y));
                        // la création d'une blackbox fabrique automatiquement des fils, que l'on veut remplacer
                        comp.wires.Clear();
                        foreach (int i in wiresId)
                        {
                            comp.wires.Add(SchemWires[i]);
                        }
                        break;
                    default:
                        throw new InvalidValueException("Type", type, "Component");
                }
                return comp;
            } else
                throw new MissingFieldException("Type or Position", "Component");
        }
    }
}
