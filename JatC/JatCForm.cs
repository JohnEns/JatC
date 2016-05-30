using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Media;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JatC
{
    public partial class JatCForm : Form
    {
        /// <summary>
        /// Random object instantie.
        /// </summary>
        private Random _random = new Random();

        /// <summary>
        /// Houdt de tijd bij waarop op de knop werpButton is geklikt.
        /// </summary>
        private DateTime _startedAt;

        /// <summary>
        /// Willekeurige tijdsinterval waarop het gooien van de dobbelsteen moet stoppen.
        /// </summary>
        private int _randomTime;

        /// <summary>
        /// Het totale aantal punten van de worp van alle 5 dobbelstenen
        /// </summary>
        private int _turnScore;

        /// <summary>
        /// De bonuspunten per ronde
        /// </summary>
        private int _bonus;

        /// <summary>
        /// Het totale aantal punten van alle worpen 
        /// </summary>
        private int _eindScore;

        /// <summary>
        /// Deze lijst houdt alle scores in 1 worp bij. Dit is speciaal voor de berekening van de Kleine Straat
        /// </summary>
        private List<int> RollScoreList = new List<int>();

        /// <summary>
        /// Het aantal worpen dat is gedaan
        /// </summary>
        private int _worpNummer;

        /// <summary>
        /// Lijst van Commentaar.
        /// </summary>
        private Dictionary<ThrowResult, string> _comment = new Dictionary<ThrowResult, string>()
        {
            {ThrowResult.Default, "Whahoo-hoo-ho!! Wat een score!" },
            {ThrowResult.FirstRoll,"Je eerste worp! Ga voor JatC!" },
            {ThrowResult.LastRoll,"Nog maar 1x gooien.. Succes!" },
            {ThrowResult.SmallStraight,"Yay! Een Kleine Straat!" },
            {ThrowResult.BigStraight,"Yay! Een Grote Straat!" },
            {ThrowResult.Yahtzee,"Yay! Je hebt JatC!!" },
            {ThrowResult.Error, "Ai! Er ging iets mis" }
        };

        /// <summary>
        /// Lijst van gezwets in Commentaar.
        /// </summary>
        private string[] _blabla = new string[]
        {
            "Ik vind dit spel echt leuk",
            "Jij moet niet naar een casino gaan!",
            "Rollende dobbelstenen .. mmmmm",
            "De volgende worp wordt beter!",
            "Zo, dat doe je goed!",
            "Word niet gokverslaafd ajb",
            "Ooooh.. wat spannend!",
            "Gotta catch 'em all!",
            "Niet te hard gooien hoor!",
            "Dit is echt gezellig",
            "JatC is het tofste spel ooit",
            "Je huiswerk is toch wel af?",
            "Komt er nu JatC?!",
            "Ik wil dit elke dag spelen!",
            "Vijf is een mooi getal",
            "Wat een leuk geluid!",
            "...en NOG een keer..!",
            "Dit is het ware geluk",
            "Lekkerder dan chocola",
            "Net een tromgeroffel",
            "Wij zijn een goed team"
        };

        /// <summary>
        /// Lijst van dobbelsteen plaatjes (zie in Solution explorer: JatC > Properties > Resources.resx.
        /// </summary>
        private Image[] _images = new Image[6]
        {
            Properties.Resources.dice1,
            Properties.Resources.dice2,
            Properties.Resources.dice3,
            Properties.Resources.dice4,
            Properties.Resources.dice5,
            Properties.Resources.dice6
        };

        /// <summary>
        /// List om alle dobbelsteen-plaatjes in te zetten. Deze zullen later dynamisch worden toegevoegd
        /// </summary>
        private static List<PictureBox> allDies = new List<PictureBox>();
                  
        /// <summary>
        /// Vul een array _labels met alle worplabels 
        /// </summary>
        private static Label[] _labels = new Label[5];

        /// <summary>
        /// Vul een array _panels met alle worppanels 
        /// </summary>
        private static Panel[] _panels = new Panel[5];

        /// <summary>
        /// Speel een audio loop af.
        /// </summary>
        /// <param name="audioLoop"></param>
        private void PlayAudioLoop(Stream audioLoop) // Speel geluid rollende dobbelsteen af in een loop
        {
            audioLoop.Position = 0; // De leeskop voor wav-loop handmatig terugzetten op 0
            var audioLoopCopy = new MemoryStream();
            audioLoop.CopyTo(audioLoopCopy);
            audioLoopCopy.Position = 0;

            using (SoundPlayer player = new SoundPlayer(audioLoopCopy))
            {
                //player.Stream = null;    // De stream verwijderen uit het geheugen
                //player.Stream = Properties.Resources.only_roll;  // De wav verbinden aan audioroll

                player.PlayLooping();          // Speel de clip af (deja vu?)
            }
        }
        /// <summary>
        /// Speel de opgegeven audioclip af.
        /// </summary>
        /// <param name="audioClip"></param>
        private void PlayAudio(Stream audioClip)
        {
            // Maak een kopie van de audio stream om het meerdere
            // keren tegelijk af te kunnen spelen
            audioClip.Position = 0;
            var audioClipCopy = new MemoryStream();
            audioClip.CopyTo(audioClipCopy);
            audioClipCopy.Position = 0;

            using (SoundPlayer player = new SoundPlayer(audioClipCopy))
            {
                player.Play();
            }
        }

        private void FillPictureBoxList()
        {
            //Vul het array met de 5 dobbelstenen plaatjes met de pictureboxes
            foreach (Control c in DobbelstenenPanel.Controls)
            {
                PictureBox pb = c as PictureBox;
                if (pb != null) allDies.Add(pb);
            }
        }

        public JatCForm()
        {
            InitializeComponent();
        }

        private void JatCForm_Load(object sender, EventArgs e)
        {
            FillLabel();

            FillPanel();

           // FillAllPanels();

            //FillDiesPanel();

            FillPictureBoxList();
            ResetAll();
        }

        /// <summary>
        /// Zet alle worplabels in een array genaamd _labels
        /// </summary>
        private void FillLabel()
        {
            _labels[0] = worp1Label;
            _labels[1] = worp2Label;
            _labels[2] = worp3Label;
            _labels[3] = worp4Label;
            _labels[4] = worp5Label;
        }

        /// <summary>
        /// Zet alle worppanels in een array genaamd _panels
        /// </summary>
        private void FillPanel()
        {
            _panels[0] = panel1;
            _panels[1] = panel2;
            _panels[2] = panel3;
            _panels[3] = panel4;
            _panels[4] = panel5;
        }

        private void ResetAll()
        {
            //Stel alle waarden op nul.
            Reset.Visible = false;
            werpButton.Visible = true;
            ScoreBereken.Visible = false;
            _turnScore = 0;
            _worpNummer = 0;
            worpNummerLabel.Text = Convert.ToString(_worpNummer);
            _eindScore = 0;
            EindScoreLabelPunten.Text = "???";
            Jenny.Image = Properties.Resources.happy;
            Commentary.Text = _comment[ThrowResult.FirstRoll];
            
            //alle labels worden leeggemaakt en gevuld met "..."
            foreach (Label l in _labels)
            {
                l.Text = "...";
            }

            //Maak alle worppanels onzichtbaar
            foreach (Panel p in _panels)
            {
                p.Visible = false;
            }
            
            DobbelstenenPanel.Visible = false;
        }

        private void werpButton_Click(object sender, EventArgs e)
        {
            //Geef de huidige worp aan
            worpNummerLabel.Text = Convert.ToString(_worpNummer);

            //Instellingen voor de tijd voorbereiden
            _randomTime = _random.Next(2000, 4000);
            _startedAt = DateTime.Now;

            //Willekeurig commentaar
            Commentary.Text = _blabla[_random.Next(0, _blabla.Length)];
            Jenny.Image = Properties.Resources.happy_closed_eyes;

            //hier komt de rol actie van de dobbelstenen
            Start();

            werpButton.Text = "Bezig...";
            werpButton.Enabled = false; // Schakel de button uit zolang de timer loopt

            timer1.Start();
        }

        /// <summary>
        /// Laat alle Dobbelstenen zien. Begin met het geluid van rollende dobbelstenen af te spelen
        /// </summary>
        private void Start()
        {
            //Dobbelstenen worden zichtbaar
            DobbelstenenPanel.Visible = true;


            PlayAudio(Properties.Resources.rolling); // Speel geluid beginnende rollende dobbelsteen af
            PlayAudioLoop(Properties.Resources.only_roll); // Speel geluid rollende dobbelsteen af in een loop

            //Er wordt een worp opgeteld aan het totaal
            _worpNummer++;
            worpNummerLabel.Text = Convert.ToString(_worpNummer);
        }

        /// <summary>
        /// Timer tick interval event handler. Gaat af elke keer als de interval van de timer voorbij is. 
        /// Stopt de timer nadat willekeurigeTijd verstreken is.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            ThrowDie();

            //Delta is het verschil tussen de willekeurige tijd en de verlopen tijd. Zodra deze voorbij is wordt de methode RollEnded gestart. 
            var delta = DateTime.Now - _startedAt;
            if (delta.TotalMilliseconds >= _randomTime)
            {
                RollEnded();
            }
        }

        //Laat voor alle dobbelstenen een random plaatje zien. Parameter P is nr plaatje dobbelsteen in de volgorde waarop deze bij initialisatie is geladen in de array.
        private void ThrowDie()
        {
            foreach (PictureBox p in allDies)
            {
                p.Image = _images[_random.Next(0, 6)];
            }
        }

        /// <summary>
        /// Dit werkt de afhandeling af van de worp
        /// </summary>
        private void RollEnded()
        {
            // Stop de timer.
            timer1.Stop();

            // Stop het rollende geluid en laat de dobbelsteen uitrollen.
            PlayAudio(Properties.Resources.long_roll);

            Jenny.Image = Properties.Resources.happy_blush;
            EvaluateResult();


            // Herstel de button zodat de dobbelsteen opnieuw geworpen kan worden.
            werpButton.Text = "Werp dobbelsteen!";
            werpButton.Enabled = true;
        }

        /// <summary>
        /// Evalueer het resultaat van de worp. Optellen van score en bonus
        /// </summary>
        private void EvaluateResult()
        {

            RollScore();
            BonusCheck();

            //Bereken de eindscore (zonder de bonus)
            _eindScore = _eindScore + _turnScore;

            // Weergave van de worp in het worplabel
            _labels[_worpNummer - 1].Text = _turnScore.ToString();

            //Laat Jenny waarschuwen dat dit je laatste worp is
            if (_worpNummer == 4)
            {
                Commentary.Text = _comment[ThrowResult.LastRoll];
            }

            //Laat Jenny waarschuwen dat dit je laatste worp is
            if (_worpNummer == 5)
            {
                LastRoll();
            }

        }

        /// <summary>
        /// Berekent de score van de worp
        /// </summary>
        private void RollScore()
        {
            //Zet _turnScore op 0 
            _turnScore = 0;

            //Introduceer variabele i om de individuele dobbelstenen te benoemen
            var i = 0;

            //Bepaal het puntenaantal van iedere dobbelsteen
            foreach (PictureBox p in allDies)
            {
                var index = Array.IndexOf(_images, p.Image);
                var score = index + 1;

                //Geef de dobbelstenen in het worppanel dezelfde waarden als de worp
                //allPanels[_worpNummer - 1][i].Image = _images[index];

                var pic = (PictureBox)_panels[_worpNummer - 1].Controls[i];
                pic.Image = _images[index];

                //Verhoog i met 1 om naar de volgende dobbelsteen te gaan
                i++;

                //Maak het worppanel zichtbaar
                _panels[_worpNummer - 1].Visible = true;

                //Tel de waarde van de dobbelsteen bij de score op
                _turnScore += score;

                //Voeg de waarde toe aan de lijst RollScoreList
                RollScoreList.Add(score);
            }
        }


        /// <summary>
        /// Deze methode berekent de eventuele bonus per worp
        /// </summary>
        private void BonusCheck()
        {
            //Bij een lage score kijkt Jenny niet zo blij
            if (_turnScore <= 14)
            {
                Jenny.Image = Properties.Resources.neutral;
            }

            //Controleer of JatC is gevallen
            if (_turnScore >= 25)
            {
                DisplayBonus(50, ThrowResult.Yahtzee);
            }

            //Controleer of er een "Straat" is gevallen
            if (RollScoreList.Distinct().Count() == RollScoreList.Count)
            {
                RollScoreList.Sort();

                if (IsStraight(1, 5)) // Kleine straat
                {
                    DisplayBonus(25, ThrowResult.SmallStraight);
                }

                if (IsStraight(2, 6)) // Grote straat
                {
                    DisplayBonus(30, ThrowResult.BigStraight);
                }
            }


            //Geef de (totale) bonus weer 
            jatCLabel.Text = _bonus.ToString();

            //Maak de list RollScoreList leeg, zodat deze weer gebruikt kan worden
            RollScoreList.Clear();
        }

        private void DisplayBonus(int bonusScore, ThrowResult bonusComment)
        {
            _bonus += bonusScore;
            PlayAudio(Properties.Resources.yay);
            Jenny.Image = Properties.Resources.surprised;
            Commentary.Text = _comment[bonusComment];
        }

        /// <summary>
        /// Controleert of de huidige worp een kleine danwel grote straat is.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private bool IsStraight(int min, int max)
        {
            return RollScoreList[0] == min && RollScoreList[RollScoreList.Count - 1] == max;
        }

        /// <summary>
        /// De totale score van alle worpen opvragen
        /// </summary>
        private void LastRoll()
        {
            //Maak ScoreBereken knop zichtbaar en klikbaar
            werpButton.Visible = false;
            ScoreBereken.Visible = true;
            ScoreBereken.Text = "Score Berekenen";
            ScoreBereken.Enabled = true;
        }

        /// <summary>
        /// Berekent de eindscore
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateScore_Click(object sender, EventArgs e)
        {
            //bereken eindscore
            _eindScore = _eindScore + _bonus;
            EindScoreLabelPunten.Text = Convert.ToString(_eindScore);
            Commentary.Text = _comment[ThrowResult.Default];
            PlayAudio(Properties.Resources.wahoohooho);
            Jenny.Image = Properties.Resources.wink;

            ScoreBereken.Enabled = false;

            Reset.Visible = true;
        }

        /// <summary>
        /// Klikken op reset start het programma opnieuw
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_Click(object sender, EventArgs e)
        {
            ResetAll();
        }


    }
}
