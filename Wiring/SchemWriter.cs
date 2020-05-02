using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Wiring
{
    static class SchemWriter
    {
        /// <summary>
        /// Ecrire à partir d'un chemin vers un fichier (ouvre un filestream)
        /// </summary>
        public static void write(string path, Schematic schem, bool indent)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("create new file : " + path);
            }
            else
            {
                Console.WriteLine("overwrite existant file : " + path);
            }
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            write(fs, schem, indent);
            fs.Close();
        }
        /// <summary>
        /// Ecrire à partir d'un FileStream
        /// </summary>
        public static void write(FileStream fs, Schematic schem, bool indent)
        {
            string code; // code représentant le schematic
            if (indent)
            {
                code = "Schematic : {\r\n" + schemToString(schem, indent, 1) + "\r\n}\r\n";
            }
            else
            {
                code = schemToString(schem, indent);
            }
            fs.SetLength(0);
            fs.Write(Encoding.UTF8.GetBytes(code), 0, Encoding.UTF8.GetByteCount(code));
        }

        private static string schemToString(Schematic schem, bool indent, int tabulation = 0)
        {
            StringBuilder code = new StringBuilder();
            if (indent)
                code.Append(tab(tabulation) + "Name : \"");
            else
                code.Append("Name:\"");
            code.Append(schem.Name);
            code.Append("\",");

            if (indent)
                code.Append("\r\n" + tab(tabulation) + "Components : [ ");
            else
                code.Append("Components:[");
            foreach (Component c in schem.components)
            {
                if (indent)
                    code.Append("\r\n" + tab(tabulation+1) + "{\r\n");
                else
                    code.Append("{");
                code.Append(compToString(c, schem.wires, indent, tabulation + 2));
                if (indent)
                    code.Append("\r\n" + tab(tabulation + 1) + "}");
                else
                    code.Append("}");
            }
            if (indent)
                code.Append("\r\n" + tab(tabulation));
            code.Append("]");

            return code.ToString();
        }
        private static string compToString(Component comp, List<Wire> wireList, bool indent, int tabulation = 0)
        {
            StringBuilder code = new StringBuilder();

            if (indent)
                code.Append(tab(tabulation) + "Type : \"");
            else
                code.Append("Type:\"");
            string[] splitedType = comp.GetType().ToString().Split('.');
            string typeName = splitedType[splitedType.Length - 1];
            code.Append(typeName);
            code.Append("\",");

            if (indent)
                code.Append("\r\n" + tab(tabulation) + "Position : [");
            else
                code.Append("Position:[");
            code.Append((int)comp.position.X + " " + (int)comp.position.Y + "],");

            if (indent)
                code.Append("\r\n" + tab(tabulation) + "Wires : [");
            else
                code.Append("Wires:[");
            for (int i = 0; i < comp.wires.Count; i++)
            {
                if (i != 0)
                    code.Append(" ");
                code.Append(wireList.IndexOf(comp.wires[i]));
            }
            code.Append("]");

            if (typeName == "Input" || typeName == "Diode" || typeName == "BlackBox")
            {
                if (indent)
                    code.Append(",\r\n" + tab(tabulation) + "Data : {\r\n" + tab(tabulation+1));
                else
                    code.Append(",Data:{");
            }
            if (comp is Input inp)
            {
                if (indent)
                    code.Append("value : ");
                else
                    code.Append("value:");
                code.Append(inp.getValue() ? "true" : "false");
            } else if (comp is Diode d)
            {
                if (indent)
                    code.Append("delay : ");
                else
                    code.Append("delay:");
                code.Append((int)d.delay);
            }
            else if (comp is BlackBox bb)
            {
                if (indent)
                    code.Append("schematic : {\r\n");
                else
                    code.Append("schematic:{");
                code.Append(schemToString(bb.schem, indent, tabulation + 2));
                if (indent)
                    code.Append("\r\n" + tab(tabulation + 1) + "}");
                else
                    code.Append("}");
            }
            if (typeName == "Input" || typeName == "Diode" || typeName == "BlackBox")
            {
                if (indent)
                    code.Append("\r\n" + tab(tabulation) + "}");
                else
                    code.Append("}");
            }

            return code.ToString();
        }
        private static string tab(int tabulation)
        {
            string str = "";
            for (int i=0; i<tabulation; i++)
            {
                str += "\t";
            }
            return str;
        }
    }
}
