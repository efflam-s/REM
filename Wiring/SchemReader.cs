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
            public SyntaxException(int i, char c, object state) : base("character n°" + i + " : " + c + ", at state : " + state) { }
        }
        public class StructureException : Exception
        {
            public StructureException(string message = "") : base(message) { }
            public StructureException(int i, string w, object state) : base("word n°" + i + " : " + w + ", at state : " + state) { }
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
        
        public static Schematic Read(string path)
        {
            string s = File.ReadAllText(path);

            string[] words = WordParser(s, true);

            int i = 0;
            Dictionary<string, object> tree = CreateTree(words, ref i, true);

            // on laisse le choix de mettre "Schematic : { Name : ... }" ou directement "Name : ..."
            if (tree.ContainsKey("Schematic") && tree["Schematic"] is Dictionary<string, object> schematic)
            {
                return TreeToSchem(schematic);
            }
            else
            {
                return TreeToSchem(tree);
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
                {
                    // error
                }
            }
            if (i >= w.Length)
            {
                throw new StructureException(i, "", "list");
            }
            start = i;
            return list;
        }

        private static Schematic TreeToSchem(Dictionary<string, object> tree)
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
                            schem.AddComponent(TreeToComp(comp, schem.wires), false);
                        } else
                        {
                            throw new InvalidValueException("Component", c, "Components");
                        }
                    }
                }
                return schem;
            }
            else
                throw new MissingFieldException("Name", "Schematic");
        }
        private static Component TreeToComp(Dictionary<string, object> tree, List<Wire> SchemWires = null)
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
                        BlackBox bb = new BlackBox(new Schematic("undefined"), new Vector2(X, Y));
                        foreach(int i in wiresId)
                        {
                            bb.wires.Add(SchemWires[i]);
                        }
                        if (tree.ContainsKey("Data") && tree["Data"] is Dictionary<string, object> data2)
                            if (data2.ContainsKey("Schematic") && data2["Schematic"] is Dictionary<string, object> schematic)
                                bb.schem = TreeToSchem(schematic);
                            else if (data2.ContainsKey("path") && data2["path"] is string path)
                                bb.schem = Read(path);
                        comp = bb;
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
