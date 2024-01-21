using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TUComparatorLibrary
{
    public class Comparator
    {
        public static List<XElement> skillData;
        public static List<XElement> factionData;

        public void Run(string oldXmlDirectory, string newXmlDirectory, string outputFolderDirectory = "./output")
        {
            string skillData = File.ReadAllText($@"{oldXmlDirectory}//skills_set.xml");
            XDocument skillDataDoc = XDocument.Parse(skillData);
            Comparator.skillData = skillDataDoc.XPathSelectElements("//root/skillType").ToList();
            Comparator.factionData = skillDataDoc.XPathSelectElements("//root/unitType").ToList();

            // load the XML files
            List<XDocument> oldCardXmls = new List<XDocument>();
            List<XDocument> newCardXmls = new List<XDocument>();

            Console.WriteLine("Reading old XMLs from file");

            // load old XMLs
            int counter = 1;
            bool lastLoadFailed = false;
            do
            {
                //try to load the file at the counter location.  If it exists, keep going.  If it doesn't, then stop
                string filename = $@"cards_section_{counter}.xml";

                try
                {
                    string file = File.ReadAllText($@"{oldXmlDirectory}//{filename}");
                    XDocument fileXml = XDocument.Parse(file);
                    oldCardXmls.Add(fileXml);
                    counter++;
                } catch(Exception e)
                {
                    lastLoadFailed = true;
                    Console.WriteLine(e.Message);
                }
            } while (!lastLoadFailed);

            Console.WriteLine($@"oldXmlDirectory found {oldCardXmls.Count} files");

            Console.WriteLine("Parsing old XMLs into card XMLs.");

            List<XElement> oldCards = new List<XElement>();
            foreach(XDocument oldCardXml in oldCardXmls)
            {
                IEnumerable<XElement> extractedCards = oldCardXml.XPathSelectElements("//root/unit");

                oldCards.AddRange(extractedCards);
            }

            Console.WriteLine($@"Found {oldCards.Count} cards in old XML.");

            Console.WriteLine($@"Parsing card XMLs into card objects");

            List<Card> oldCardObjects = new List<Card>();
            foreach(XElement oldCard in oldCards)
            {
                oldCardObjects.Add(new Card(oldCard));
            }

            Console.WriteLine($@"{oldCardObjects.Count} cards parsed into objects");




            Console.WriteLine("Reading new XMLs from file");

            // load old XMLs
            counter = 1;
            lastLoadFailed = false;
            do
            {
                //try to load the file at the counter location.  If it exists, keep going.  If it doesn't, then stop
                string filename = $@"cards_section_{counter}.xml";

                try
                {
                    string file = File.ReadAllText($@"{newXmlDirectory}//{filename}");
                    XDocument fileXml = XDocument.Parse(file);
                    newCardXmls.Add(fileXml);
                    counter++;
                }
                catch (Exception e)
                {
                    lastLoadFailed = true;
                    Console.WriteLine(e.Message);
                }
            } while (!lastLoadFailed);

            Console.WriteLine($@"newXmlDirectory found {newCardXmls.Count} files");

            Console.WriteLine("Parsing new XMLs into card XMLs.");

            List<XElement> newCards = new List<XElement>();
            foreach (XDocument newCardXml in newCardXmls)
            {
                IEnumerable<XElement> extractedCards = newCardXml.XPathSelectElements("//root/unit");

                newCards.AddRange(extractedCards);
            }

            Console.WriteLine($@"Found {newCards.Count} cards in new XML.");

            Console.WriteLine($@"Parsing card XMLs into card objects");

            List<Card> newCardObjects = new List<Card>();
            foreach (XElement newCard in newCards)
            {
                newCardObjects.Add(new Card(newCard));
            }

            Console.WriteLine($@"{newCardObjects.Count} cards parsed into objects");

            Console.WriteLine($@"Building comparison...");

            List<SingleComparison> comparisons = new List<SingleComparison>();

            foreach (Card newCard in newCardObjects)
            {
                Card oldCardEquivalent = oldCardObjects.Where(x => x.id == newCard.id).FirstOrDefault();

                if(oldCardEquivalent != null)
                {
                    // found a match
                    comparisons.Add(new SingleComparison(oldCardEquivalent, newCard));

                    oldCardObjects.Remove(oldCardEquivalent);
                }
                else
                {
                    // no old card equivalent.  card is new.
                    comparisons.Add(new SingleComparison(null, newCard));
                }
            }

            // loop over whatever old cards remain.  They must have been removed.
            foreach(Card oldCard in oldCardObjects)
            {
                comparisons.Add(new SingleComparison(oldCard, null));
            }

            Console.WriteLine($@"{comparisons.Count} comparisons build.  Comparing...");

            var addList = comparisons.Where(x => x.GetType() == SingleComparison.ComparisonType.ADDED).ToList();
            var removeList = comparisons.Where(x => x.GetType() == SingleComparison.ComparisonType.REMOVED).ToList();
            var changedList = comparisons.Where(x => x.GetType() == SingleComparison.ComparisonType.CHANGED).ToList();

            StringBuilder results = new StringBuilder();

            if(addList.Count > 0)
            {
                results.AppendLine("New Cards:");
                foreach(var add in addList)
                {
                    results.Append(add.Compare());
                }
                results.AppendLine();
                results.AppendLine();
            }

            if (removeList.Count > 0)
            {
                results.AppendLine("Removed Cards:");
                foreach (var remove in removeList)
                {
                    results.Append(remove.Compare());
                }
                results.AppendLine();
                results.AppendLine();
            }

            if(changedList.Count > 0)
            {
                // intermediate, as we have this list before we know if there are changes.
                StringBuilder changedBuilder = new StringBuilder();

                foreach(var changed in changedList)
                {
                    changedBuilder.Append(changed.Compare());
                }

                if(changedBuilder.Length > 0)
                {
                    results.AppendLine("Changed Cards:");
                    results.Append(changedBuilder.ToString());
                }
                results.AppendLine();
                results.AppendLine();
            }

            Console.WriteLine("Results:");

            Directory.CreateDirectory(outputFolderDirectory);
            File.WriteAllText($@"{outputFolderDirectory}/output-{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}.txt", results.ToString());

            Console.WriteLine(results.ToString());
        }
    }
}
