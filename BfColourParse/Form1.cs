using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BfColourParse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var input = txtInput.Text;
            input = input.Replace("<br>", "\n\n");
            input = Regex.Replace(input, "<color=0:255:0>(.+?)</color>", "[$1](/gg)");
            input = Regex.Replace(input, "<color=0:0:255>(.+?)</color>", "[$1](/gt)");
            input = Regex.Replace(input, "<color=128:0:128>(.+?)</color>", "[$1](/ca)");
            input = Regex.Replace(input, "<color=255:0:0>(.+?)</color>", "[$1](/tg)");
            input = Regex.Replace(input, "<color=255:255:0>(.+?)</color>", "[$1](/ta)");
            input = Regex.Replace(input, "<color=192:192:192>(.+?)</color>", "[$1](/cg)");
            input = HtmlRemoval.StripTagsRegexCompiled(input);
            txtOutput.Text = input;
        }
    }

    public static class HtmlRemoval
    {
        /// <summary>
        /// Remove HTML from string with Regex.
        /// </summary>
        public static string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Compiled regular expression for performance.
        /// </summary>
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        public static string StripTagsRegexCompiled(string source)
        {
            return _htmlRegex.Replace(source, string.Empty);
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>
        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}
