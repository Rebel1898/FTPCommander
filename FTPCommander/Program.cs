using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace FTPCommander
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument config = new XmlDocument();
            string configPath = @"..\..\..\App.config";
            config.Load(configPath);

            string Comando = config.GetElementsByTagName("command")[0].InnerText;
            string rutaLocal = config.GetElementsByTagName("localPath")[0].InnerText;
            string rutaTemota = config.GetElementsByTagName("remotePath")[0].InnerText;
            string subcarpetaRemota = config.GetElementsByTagName("remoteSubFolder")[0].InnerText;
            string user = config.GetElementsByTagName("user")[0].InnerText;
            string pass = config.GetElementsByTagName("password")[0].InnerText;
            string nuevaCarpeta = config.GetElementsByTagName("newRemoteFolder")[0].InnerText;
            string namePattern = config.GetElementsByTagName("namePattern")[0].InnerText;
            string StartDateString = config.GetElementsByTagName("startDate")[0].InnerText;
            string EndDateString = config.GetElementsByTagName("endDate")[0].InnerText;
            int typePattern = Int32.Parse(config.GetElementsByTagName("typePattern")[0].InnerText);
            string localFilepathtoUpload = config.GetElementsByTagName("localFilepathtoUpload")[0].InnerText;
            string NewRemoteFolderName = config.GetElementsByTagName("NewRemoteFolderName")[0].InnerText;
            string FolderToRename = config.GetElementsByTagName("FolderToRename")[0].InnerText;
            
            DateTime ZeroDate;
            DateTime StartDate;
            DateTime EndDate;
            DateTime.TryParse(StartDateString, out StartDate);
            DateTime.TryParse(EndDateString, out EndDate);
            DateTime.TryParse("nothing", out ZeroDate);

            NetworkCredential clave = new System.Net.NetworkCredential(user, pass);
            try
            {
                using (FtpClient ftp = new FtpClient(rutaTemota, clave))
                {
                    Console.WriteLine("FTP succesfully connected!");
                    ftp.SetWorkingDirectory(subcarpetaRemota);


                    if (Comando == "Download")
                    {
                        DescargarArchivosconPatron();
                    }
                    if (Comando == "CreateRemoteFolder")
                    {
                        CrearDirectorio(nuevaCarpeta);
                        Console.WriteLine(nuevaCarpeta + " Created");
                    }
                    if (Comando == "UploadFile")
                    {
                        UploadFile(localFilepathtoUpload);
                        Console.WriteLine(localFilepathtoUpload + " uploaded to " + rutaTemota + "/" + subcarpetaRemota);

                    }
                    if (Comando == "Rename")
                    {
                        RenameFileorFolder(FolderToRename, NewRemoteFolderName);
                        Console.WriteLine("Folder" + FolderToRename + "was renamed to " + NewRemoteFolderName);

                    }
                    if (Comando == "UploadFolder")
                    {
                        UploadDir(rutaLocal);
                        Console.WriteLine("The folder " + rutaLocal + "was uploaded to  " + rutaTemota + "/" + subcarpetaRemota);
                    }
                    else
                    {
                        Console.WriteLine("Invalid command");
                    }

                    ftp.Disconnect();

                    void DescargarArchivosconPatron()
                    {
                        Console.WriteLine("Pattern to Download " + namePattern);
                        Console.WriteLine("Type to Download " + typePattern);
                        Console.WriteLine("Start Date " + StartDate);
                        Console.WriteLine("End Date " + EndDate);

                        FtpListItem[] listado = ftp.GetListing();
                        IEnumerable<FtpListItem> listadoFiltrado = listado.Where(x => Regex.Match(x.Name, namePattern).Success == true);
                        if (EndDateString == "" || EndDate == ZeroDate)
                            listadoFiltrado = listadoFiltrado.Where(x => x.RawModified.Date >= StartDate);
                        else
                            listadoFiltrado = listadoFiltrado.Where(x => x.RawModified.Date >= StartDate && x.RawModified.Date <= EndDate);

                        foreach (FtpListItem ftpitem in listadoFiltrado)
                        {
                            if (ftpitem.Type == FtpFileSystemObjectType.File && typePattern == 1 || typePattern == 3)
                            {
                                ftp.DownloadFile(rutaLocal + ftpitem.Name, ftpitem.Name);
                                Console.WriteLine(ftpitem.Name + " was downloaded to " + rutaLocal + " \r\n");
                            }
                            if (ftpitem.Type == FtpFileSystemObjectType.Directory && typePattern >= 2)
                            {
                                ftp.DownloadDirectory(rutaLocal + ftpitem.Name, ftpitem.Name);
                                Console.WriteLine(ftpitem.Name + " was downloaded to " + rutaLocal + " \r\n");
                            }
                        }
                    }

                    void CrearDirectorio(string NewFolder)
                    {
                        if (!ftp.DirectoryExists(NewFolder))
                            ftp.CreateDirectory(NewFolder);
                    }

                    void UploadFile(string Filename)
                    {
                        string currentDir = ftp.GetWorkingDirectory() + "/";
                        ftp.UploadFile(rutaLocal + Filename, currentDir + Filename, FtpRemoteExists.Skip);
                    }

                    void RenameFileorFolder(string Remotename, string newName)
                    {
                        string currentDir = ftp.GetWorkingDirectory() + "/";
                        ftp.MoveDirectory(currentDir + Remotename, currentDir + newName, FtpRemoteExists.Skip);
                    }

                    void UploadDir(string DirPath)
                    {
                        string currentDir = ftp.GetWorkingDirectory() + "/";
                        if (!ftp.DirectoryExists(currentDir + DirPath))
                            ftp.UploadDirectory(DirPath, currentDir);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR" + ex.Message.ToString());
            }
        }
    }
}

