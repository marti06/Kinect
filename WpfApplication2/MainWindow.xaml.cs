using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Net.Mail;

namespace Kinect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor myKinectSensor = null;
        private Body[] bodies;
        private BodyFrameReader bodyFrameReader = null;
        private List<Tuple<JointType, JointType>> bones;
        private string fallen = null;
       
        public MainWindow()
        {
            this.myKinectSensor = KinectSensor.GetDefault();

            this.bodyFrameReader = this.myKinectSensor.BodyFrameSource.OpenReader();

            this.bones = new List<Tuple<JointType, JointType>>();
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            
            this.myKinectSensor.Open();

            this.InitializeComponent();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void isLoaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        /*private void closed(object sender, EventArgs e)
        {
            this.myKinectSensor.Close();
        }*/

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }
            if (dataReceived)
            {
                string podaci = string.Empty;
                string fallenPersons = string.Empty;
                var counter = 1;
                var counterFallen = 0;
                var count = bodies.Count(it => it.IsTracked);
                bodiesCount.Content = count;
                foreach (Body body in this.bodies)
                {
                    // Only process tracked bodies
                    if (body.IsTracked)
                    {
                        
                        Joint head = body.Joints[JointType.Head];
                        double x = Math.Round(head.Position.X, 3);
                       // xHead.Content = x;
                        double y = Math.Round(head.Position.Y, 3);
                        //yHead.Content = y;
                        double z = Math.Round(head.Position.Z, 3);
                        //zHead.Content = z;
                        podaci += string.Format("Osoba " + counter + " na koordinatama: " + "x: " + x + " y: " + y + " z: " + z) + Environment.NewLine;
                        multiLineTextBox.Text = podaci;

                        Joint spine = body.Joints[JointType.SpineMid];
                        float xSpine = spine.Position.X;
                        float ySpine = spine.Position.Y;
                        float zSpine = spine.Position.Z;
                        Joint ankle = body.Joints[JointType.AnkleLeft];
                        float yAnkle = ankle.Position.Y;
                       
                        pad.Text = "Osoba nije pala";
                       
                        if (0 - ySpine > 0.9)
                        {
                            fallenPersons += string.Format("Osoba " + counter + " je pala") + Environment.NewLine;
                            pad.Text = fallenPersons;
                            
                            if (fallen == null || counterFallen > 200 || counterFallen > 400)
                            {
                                MailMessage mM = new MailMessage();
                                //Mail Address
                                mM.From = new MailAddress("martina.bicanic@gmail.com");
                                //receiver email id
                                mM.To.Add("martina.bicanic@gmail.com");
                                //subject of the email
                                mM.Subject = "Detekcija pada";
                                //add the body of the email
                                mM.Body = "Osoba je pala";
                                mM.IsBodyHtml = true;
                                //SMTP client
                                SmtpClient sC = new SmtpClient("smtp.gmail.com");
                                //port number for Gmail mail
                                sC.Port = 587;
                                //credentials to login in to Gmail account
                                sC.Credentials = new System.Net.NetworkCredential("martina.bicanic@gmail.com", "140791");
                                //enabled SSL
                                sC.EnableSsl = true;
                                //Send an email
                                sC.Send(mM);
                            }         
                            fallen = "shit just got real";
                            counterFallen++;
                        }
                       
                        counter++;
                    }
                    
                }
            }
        }

        private void isFallen()
        {

        }

    }
}
