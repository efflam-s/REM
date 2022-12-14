using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using REM.Wiring;

namespace REM
{
    /// <summary>
    /// Permet de d'écrire le code d'un schematic dans un fichier .schem à partir de l'objet
    /// </summary>
    static class SchemWriter
    {
        /// <summary>
        /// Ecrire à partir d'un chemin vers un fichier (ouvre un filestream)
        /// </summary>
        public static void write(string path, Schematic schem, bool indent, bool dontWriteBlackbox)
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
            write(fs, schem, indent, dontWriteBlackbox);
            fs.Close();
        }
        /// <summary>
        /// Ecrire à partir d'un FileStream
        /// </summary>
        public static void write(FileStream fs, Schematic schem, bool indent, bool dontWriteBlackbox)
        {
            string code; // code représentant le schematic
            if (indent)
            {
                code = "Schematic : {\r\n" + schemToString(schem, indent, dontWriteBlackbox, 1) + "\r\n}\r\n";
            }
            else
            {
                code = schemToString(schem, indent, dontWriteBlackbox);
            }
            fs.SetLength(0);
            fs.Write(Encoding.UTF8.GetBytes(code), 0, Encoding.UTF8.GetByteCount(code));
        }

        /// <summary>
        /// Ecrit le code d'un Schematic dans un string
        /// </summary>
        /// <param name="indent">Si on doit retourner à la ligne et indenter le texte</param>
        /// <param name="tabulation">Niveau d'indentation actuel</param>
        private static string schemToString(Schematic schem, bool indent, bool dontWriteBlackbox, int tabulation = 0)
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
                code.Append(compToString(c, schem.wires, indent, dontWriteBlackbox, tabulation + 2));
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
        /// <summary>
        /// Ecrit le code d'un Composant dans un string
        /// </summary>
        /// <param name="wireList">Liste des wires du Schematic. Permet de les transformer en leur id</param>
        /// <param name="indent">Si on doit retourner à la ligne et indenter le texte</param>
        /// <param name="tabulation">Niveau d'indentation actuel</param>
        private static string compToString(Component comp, List<Wire> wireList, bool indent, bool dontWriteBlackbox, int tabulation = 0)
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
                if (indent)
                    code.Append(",\r\n" + tab(tabulation) + "value : ");
                else
                    code.Append(",value:");
                code.Append(d.GetOutput(d.wires[1]) ? "true" : "false");
            }
            else if (comp is BlackBox bb)
            {
                if (dontWriteBlackbox)
                {
                    if (indent)
                        code.Append("path : \"");
                    else
                        code.Append("path:\"");
                    code.Append(bb.schem.Name);
                    code.Append(".schem\"");
                }
                else
                {
                    if (indent)
                        code.Append("schematic : {\r\n");
                    else
                        code.Append("schematic:{");
                    code.Append(schemToString(bb.schem, indent, false, tabulation + 2));
                    if (indent)
                        code.Append("\r\n" + tab(tabulation + 1) + "}");
                    else
                        code.Append("}");
                }
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
