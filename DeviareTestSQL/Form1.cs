using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nektra.Deviare2;

namespace DeviareTestSQL
{
    public partial class Form1 : Form
    {

        private NktSpyMgr _spyMgr;
        private NktProcess _process;
        private object continueEvent;
        //private Boolean bTrace = false;
        private int iCounter = -1;
        //HRESULT hRes;
        //CComVariant cVt, CVtContinueEv;

        public Form1(string ProgramName)
        {
            InitializeComponent();

            _spyMgr = new NktSpyMgr();
            _spyMgr.Initialize();
            _process = _spyMgr.CreateProcess(ProgramName, true, out continueEvent);
            //hRes = _spyMgr.CreateProcess("C:\windows\system32\notepad.exe", VARIANT_TRUE, &CVtContinueEv, &cProc);

            //_spyMgr.OnFunctionCalled += new DNktSpyMgrEvents_OnFunctionCalledEventHandler(OnFunctionCalled);

            //_process = GetProcess(ProgramName);
            if (_process == null)
            {
                //MessageBox.Show("Please start " + ProgramName + " before!", "Error");
                MessageBox.Show("Process creation " + ProgramName + "failed", "Error");
                Environment.Exit(0);
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //NktHook hook = _spyMgr.CreateHook("msvcrt.dll!strcat_s", (int)(eNktHookFlags.flgRestrictAutoHookToSameExecutable | eNktHookFlags.flgOnlyPreCall));
            //_spyMgr.OnFunctionCalled += new DNktSpyMgrEvents_OnFunctionCalledEventHandler(OnFunctionCalled);
            NktHook hook = _spyMgr.CreateHook("msvcrt.dll!wcschr", (int)(eNktHookFlags.flgRestrictAutoHookToSameExecutable | eNktHookFlags.flgOnlyPreCall));
            _spyMgr.OnFunctionCalled += new DNktSpyMgrEvents_OnFunctionCalledEventHandler(WcsCharOnFunctionCalled);
            //NktHook hook = _spyMgr.CreateHook("sqlsrv32.dll!SQLDriverConnectW", (int)(eNktHookFlags.flgRestrictAutoHookToSameExecutable | eNktHookFlags.flgOnlyPreCall));
            //_spyMgr.OnFunctionCalled += new DNktSpyMgrEvents_OnFunctionCalledEventHandler(SQLOnFunctionCalled);

            hook.Hook(true);
            hook.Attach(_process, true);
            _spyMgr.ResumeProcess(_process, continueEvent);
            this.tbxOutput.AppendText("Monitoring started");
        }

        private NktProcess GetProcess(string proccessName)
        {
            NktProcessesEnum enumProcess = _spyMgr.Processes();
            NktProcess tempProcess = enumProcess.First();
            while (tempProcess != null)
            {
                if (tempProcess.Name.Equals(proccessName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return tempProcess;
                }
                tempProcess = enumProcess.Next();
            }
            return null;
        }

        private void WcsCharOnFunctionCalled(NktHook hook, NktProcess process, NktHookCallInfo hookCallInfo)
        {
            //string strCallstrcat = "Source:";
            const string SQL_String = "SQL Server";
            string paramString;
            string strInstance="";
            string strUser="";
            string strPassword="";
            string strDatabase="";

            INktParamsEnum paramsEnum = hookCallInfo.Params();

            //lpDestionation
            INktParam param = paramsEnum.First();
            paramString = param.ReadString();

            if (paramString.CompareTo(SQL_String) == 0)
                if (iCounter == -1)
                {
                    //first time we seen "SQL Server"
                    iCounter = 0;
                }
                else
                {//Second time for "SQL Server" String
                    iCounter++;
                }
            else
            {// we are in an SQL Server Connection String
                if (iCounter > 0)
                {
                    switch (iCounter)
                    {
                        case 1:
                            {
                                Console.Write("SQL Instance ");
                                Console.WriteLine(paramString);
                                strInstance= paramString;
                                break;
                            }
                        case 3:
                            {
                                Console.Write("User ");
                                Console.WriteLine(paramString);
                                strUser = paramString;
                                break;
                            }
                        case 5:
                            {
                                Console.Write("Password ");
                                Console.WriteLine(paramString);
                                strPassword = paramString;
                                break;
                            }
                        case 7:
                            {
                                Console.Write("Database ");
                                Console.WriteLine(paramString);
                                strDatabase = paramString;
                                //dataGridView1.Rows.Add(strInstance, strUser, strPassword, strDatabase);
                                strInstance = "";
                                strUser = "";
                                strPassword = "";
                                strDatabase = "";
                                iCounter = -2;
                                break;
                            }
                    }
                    iCounter++;
                }
            }
            //this.textBox1.AppendText(strCallstrcat);
        }
        private void OnFunctionCalled(NktHook hook, NktProcess process, NktHookCallInfo hookCallInfo)
        {
            string strCallstrcat = "Destination:";

            INktParamsEnum paramsEnum = hookCallInfo.Params();

            //lpDestionation
            INktParam param = paramsEnum.First();
            strCallstrcat += param.ReadString() + " NumberOfElements:";

            //size_T 
            param = paramsEnum.Next();
            strCallstrcat += param.SizeTVal + " Source:";
            //lpSource
            param = paramsEnum.Next();
            strCallstrcat += param.ReadString();

            Console.WriteLine(strCallstrcat);
            //this.textBox1.AppendText(strCallstrcat);
        }

        private void SQLOnFunctionCalled(NktHook hook, NktProcess process, NktHookCallInfo hookCallInfo)
        {
            string strCall = "SQLDriverConnectW:";

            INktParamsEnum paramsEnum = hookCallInfo.Params();

            //Get Connection String
            INktParam param = paramsEnum.First();
            param = paramsEnum.Next();
            param = paramsEnum.Next();
            strCall += param.ReadString();

            //size_T 
            //            param = paramsEnum.Next();
            //           strCallstrcat += param.SizeTVal + " Source:";
            //lpSource
            //            param = paramsEnum.Next();
            //            strCallstrcat += param.ReadString();

            Console.WriteLine(strCall);
            //this.textBox1.AppendText(strCallstrcat);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

    }
}
