///Jquery-FileUpload-DotNet: https://github.com/fpaganini/jQuery-File-Upload-DotNET
///Original Project: https://github.com/blueimp/jQuery-File-Upload
/// Powered by: https://github.com/fpaganini
/// Special Thanks: https://github.com/blueimp
/// 2015-11-10 - pgnSoft.com

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;

namespace jQueryFileUploadDotNET.server
{
    /// <summary>
    /// Summary description for handler
    /// </summary>
    public class handler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {

        /// <summary>
        /// The modes to save file
        /// </summary>
        public enum SaveFileModes
        {
            /// <summary>
            /// Dont save the file in the disk. Just save in session for after, you can save in database. You decide if you save in database at upload moment or after user click in a save buton than you create.
            /// You can use (List<jQueryFileUploadDotNET.server.File>)Session["jquery_fileupload_dotnet_files"] to get these files.
            /// </summary>
            MemoryStream,
            /// <summary>
            /// Dont use memory to alocate file, save directly in disk.
            /// </summary>
            FileStream
        }

        /// <summary>
        /// Set the savefile mode. See SaveFileModes enum comentaries
        /// </summary>
        public SaveFileModes SaveMode { get; set; }

        /// <summary>
        /// The entry point
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            string ContextFileName = context.Request["f"];
            string ContextFileName_Thumb = context.Request["t"];
            string ContextFileName_Delete = context.Request["d"];
            if (string.IsNullOrEmpty(ContextFileName) == false)
            {
                //Read the original file
                MemoryStream file = null;
                string FileType = string.Empty;

                if (SaveMode == SaveFileModes.MemoryStream)
                {
                    if (context.Session["jquery_fileupload_dotnet_files"] != null)
                    {
                        List<File> files = (List<jQueryFileUploadDotNET.server.File>)context.Session["jquery_fileupload_dotnet_files"];
                        foreach (var item in files)
                        {
                            if (item.GetIniqueID() == ContextFileName)
                            {
                                file = item.GetFileStream();
                                FileType = item.type;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    ///Read a unique file from disk is not implemented
                    /// TODO: Implements read unique file from disk
                    throw new NotImplementedException();
                }

                if (file == null)
                {
                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "File not found";
                    context.Response.End();
                    return;
                }
                else
                {
                    //write file to response
                    context.Response.ContentType = FileType;
                    file.WriteTo(context.Response.OutputStream);
                    context.Response.End();
                }
            }
            else if (string.IsNullOrEmpty(ContextFileName_Thumb) == false)
            {
                //Thumbnail

                //Read the thumb file
                Stream file = null;
                string FileType = string.Empty;

                if (SaveMode == SaveFileModes.MemoryStream)
                {
                    if (context.Session["jquery_fileupload_dotnet_files"] != null)
                    {
                        List<File> files = (List<jQueryFileUploadDotNET.server.File>)context.Session["jquery_fileupload_dotnet_files"];
                        foreach (var item in files)
                        {
                            if (item.GetIniqueID() == ContextFileName_Thumb)
                            {
                                file = item.GetThumbStream();
                                FileType = item.type;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    ///Read a unique file from disk is not implemented
                    /// TODO: Implements read unique file from disk
                    throw new NotImplementedException();
                }

                if (file == null)
                {
                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "File not found";
                    context.Response.End();
                    return;
                }
                else
                {
                    //write file to response
                    context.Response.ContentType = FileType;
                    MemoryStream ms = new MemoryStream();

                    byte[] buffer = new byte[16 * 1024];
                    int read;
                    while ((read = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    file.Position = 0;
                    ms.WriteTo(context.Response.OutputStream);
                    context.Response.End();
                }


            }
            else if (string.IsNullOrEmpty(ContextFileName_Delete) == false)
            {
                //Delete the selected file
                if (SaveMode == SaveFileModes.MemoryStream)
                {
                    if (context.Session["jquery_fileupload_dotnet_files"] != null)
                    {
                        List<File> files = (List<jQueryFileUploadDotNET.server.File>)context.Session["jquery_fileupload_dotnet_files"];
                        List<File> NewList = new List<File>();
                        foreach (var item in files)
                        {
                            if (item.GetIniqueID() == ContextFileName_Delete)
                            {
                            }
                            else
                            {
                                NewList.Add(item);
                            }
                        }
                        context.Session["jquery_fileupload_dotnet_files"] = NewList;
                    }
                }
                else
                {
                    ///Delete a file from disk is not implemented
                    ///TODO: Implements delete file from disk
                    throw new NotImplementedException();
                }
            }

            if (context.Request.HttpMethod.ToUpper() == "GET")
            {
                if (string.IsNullOrEmpty(ContextFileName) == true)
                {
                    //Read the entire server files
                    if (SaveMode == SaveFileModes.MemoryStream)
                    {
                        if (context.Session["jquery_fileupload_dotnet_files"] != null)
                        {
                            List<File> files = (List<jQueryFileUploadDotNET.server.File>)context.Session["jquery_fileupload_dotnet_files"];
                            SerializeFiles(context.Response, files);
                        }
                    }
                    else
                    {
                        ///Read files from disk is not implemented
                        ///TODO: Implements read files from disk
                        throw new NotImplementedException();
                    }


                }
            }
            else if (context.Request.HttpMethod.ToUpper() == "POST")
            {
                //Upload the files
                List<File> AllFiles = (List<jQueryFileUploadDotNET.server.File>)context.Session["jquery_fileupload_dotnet_files"];
                if (AllFiles == null)
                    AllFiles = new List<File>();

                List<File> NewsFiles = new List<File>();

                for (int index = 0; index < context.Request.Files.Count; index++)
                {
                    HttpPostedFile File = context.Request.Files[index];
                    if (SaveMode == SaveFileModes.MemoryStream)
                    {
                        NewsFiles.Add(new File(File.FileName, File.ContentLength, File.ContentType, context.Session.SessionID, File.InputStream));
                    }
                    else
                    {
                        /// Save files to disk is not implemented
                        /// TODO: Implements save files from disk
                        throw new NotImplementedException();
                    }
                }

                AllFiles.AddRange(NewsFiles);
                context.Session["jquery_fileupload_dotnet_files"] = AllFiles;

                SerializeFiles(context.Response, NewsFiles);
            }
            else if (context.Request.HttpMethod.ToUpper() == "DELETE")
            {
            }
            else
            {
                context.Response.StatusCode = 405;
                context.Response.StatusDescription = "Method not allowed";
                context.Response.End();
                return;
            }

            context.Response.End();
        }

        /// <summary>
        /// Function to serialize the files object into a valid json string
        /// </summary>
        /// <param name="response">The current Context.Response</param>
        /// <param name="fileResponseList">The files</param>
        private void SerializeFiles(HttpResponse response, List<File> fileResponseList)
        {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = new Result(fileResponseList);
            var jsonObj = js.Serialize(result);
            response.Write(jsonObj.ToString());
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    #region ResultPatern

    /// <summary>
    /// Represent a single file result. This schema are read by front-end and fill 'template-download' filds 
    /// </summary>
    public class File
    {
        /// <summary>
        /// The display name of file
        /// </summary>
        public string name { get; set; }
        
        /// <summary>
        /// The url than 'template-download' use to show a thumbnail. Can use a original file or an icon or a small version of original image.
        /// </summary>
        public string thumbnailUrl { get; set; }

        /// <summary>
        /// The size of file in bytes
        /// </summary>
        public long size { get; set; }
        
        /// <summary>
        /// The type of file
        /// </summary>
        public string type { get; set; }
        
        /// <summary>
        /// The url to access the original file to download
        /// </summary>
        public string url { get; set; }
        
        /// <summary>
        /// The server error descrition
        /// </summary>
        public string error { get; set; }
        
        /// <summary>
        /// The url used to call the server to delete this especific file
        /// </summary>
        public string deleteUrl { get; set; }
        
        /// <summary>
        /// The delete method used. (DELETE, POST, GET)
        /// </summary>
        public string deleteType { get; set; }
        
        /// <summary>
        /// Represent if delet needs credentials
        /// </summary>
        public string deleteWithCredentials { get; set; }

        /// <summary>
        /// The unique id of file generate by server. This ID is not over 250 characters
        /// </summary>
        private string UniqueID { get; set; }

        /// <summary>
        /// The session id guarantees than the files get see olny by the actual session (user)
        /// </summary>
        private string SessionID { get; set; }

        /// <summary>
        /// The stream of file
        /// </summary>
        private MemoryStream FileStream { get; set; }

        /// <summary>
        /// Get a Thumb of original image or a incon than represent the file
        /// </summary>
        private MemoryStream ThumbStream { get; set; }

        /// <summary>
        /// Get the IniqueID of this file. It's used to distinct the files uploaded if the name of file are the same.
        /// </summary>
        /// <returns></returns>
        public string GetIniqueID()
        {
            if (string.IsNullOrEmpty(UniqueID))
            {
                UniqueID = name + DateTime.Now.Ticks.ToString() + Guid.NewGuid();
                if (UniqueID.Length > 250)
                {
                    UniqueID = UniqueID.Substring(UniqueID.Length - 250, 250);
                }
            }
            return UniqueID;
        }

        /// <summary>
        /// Get the ID of Session. This is used only in FileSave mode, for distinct what files are for the current user session.
        /// </summary>
        /// <returns></returns>
        public string GetSessionID()
        {
            return SessionID;
        }

        /// <summary>
        /// The current file in MemoryStream
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetFileStream()
        {
            return this.FileStream;
        }

        /// <summary>
        /// The Thumb or icon for the current file
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetThumbStream()
        {
            return ThumbStream;
        }

        /// <summary>
        /// Create a new instance of file
        /// </summary>
        /// <param name="name">The real name of file</param>
        /// <param name="url">The URL used to acces the real file</param>
        /// <param name="size">The size of file</param>
        /// <param name="type">the type of file (mime type)</param>
        /// <param name="thumbnailUrl">The url used to acces the thumb or icon version of file</param>
        /// <param name="deleteURL">The url used to send to server a comand to delete the especific file</param>
        /// <param name="deleteType">The delete moede (POST,GET,DELETE)</param>
        /// <param name="deleteWithCredentials">Set if chredentials are used to delete the file</param>
        /// <param name="SessionID">The id of current context.session (used only in FileSave mode, for distinct what files are for the current user session.) </param>
        /// <param name="error">The error description, if needed</param>
        public File(string name, string url, long size, string type, string thumbnailUrl, string deleteURL, string deleteType, string deleteWithCredentials, string SessionID, string error)
        {
            this.name = name;
            this.url = url;
            this.size = size;
            this.type = type;
            this.thumbnailUrl = thumbnailUrl;
            this.deleteUrl = deleteURL;
            this.deleteType = deleteType;
            this.deleteWithCredentials = deleteWithCredentials;
            this.error = error;
            this.SessionID = SessionID;
            UniqueID = GetIniqueID();

        }

        /// <summary>
        /// Create a new instance of file
        /// </summary>
        /// <param name="name">The real name of file</param>
        /// <param name="size">The size of file</param>
        /// <param name="type">The type of file (mime type)</param>
        /// <param name="SessionID">The id of current context.session (used only in FileSave mode, for distinct what files are for the current user session.) </param>
        public File(string name, long size, string type, string SessionID) : this(name, size, type, SessionID, null, null)
        {
        }

        /// <summary>
        /// Create a new instance of file
        /// </summary>
        /// <param name="name">The real name of file</param>
        /// <param name="size">The size of file</param>
        /// <param name="type">The type of file (mime type)</param>
        /// <param name="SessionID">The id of current context.session (used only in FileSave mode, for distinct what files are for the current user session.) </param>
        /// <param name="FileStream">The stream of file</param>
        public File(string name, long size, string type, string SessionID, Stream FileStream) : this(name, size, type, SessionID, FileStream, null)
        {
        }

        /// <summary>
        /// Create a new instance of file
        /// </summary>
        /// <param name="name">The real name of file</param>
        /// <param name="size">The size of file</param>
        /// <param name="type">the type of file (mime type)</param>
        /// <param name="SessionID">The id of current context.session (used only in FileSave mode, for distinct what files are for the current user session.) </param>
        /// <param name="error">The error description, if needed</param>
        public File(string name, long size, string type, string SessionID, string error) : this(name, size, type, SessionID, null, error)
        {
        }

        /// <summary>
        /// Create a new instance of file
        /// </summary>
        /// <param name="name">The real name of file</param>
        /// <param name="size">The size of file</param>
        /// <param name="type">the type of file (mime type)</param>
        /// <param name="SessionID">The id of current context.session (used only in FileSave mode, for distinct what files are for the current user session.) </param>
        /// <param name="FileStream">The stream of file</param>
        /// <param name="error">The error description, if needed</param>
        public File(string name, long size, string type, string SessionID, Stream FileStream, string error)
        {
            this.name = name;
            UniqueID = GetIniqueID();

            this.url = "server/handler.ashx?f=" + UniqueID; ;
            this.size = size;
            this.type = type;
            this.thumbnailUrl = "server/handler.ashx?t=" + UniqueID; ;
            this.deleteUrl = "server/handler.ashx?d=" + UniqueID;
            this.deleteType = "POST";
            this.deleteWithCredentials = null;
            this.error = error;
            this.SessionID = SessionID;

            byte[] buffer = new byte[16 * 1024];
            int read;
            this.FileStream = new MemoryStream();
            while ((read = FileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                this.FileStream.Write(buffer, 0, read);
            }


            try
            {
                Image image = Image.FromStream(FileStream);
                Image thumb = image.GetThumbnailImage(80, (int)(((80.0 / (double)image.Width)  ) * (double)image.Height)    , () => false, IntPtr.Zero);
                ThumbStream = new MemoryStream();
                thumb.Save(ThumbStream, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception)
            {
                //Thumbnail not create if the type of file is not an image
                //TODO: Create a incon to file
            }

            if (FileStream != null)
                FileStream.Position = 0;

            if (ThumbStream != null)
                ThumbStream.Position = 0;
        }
    }


    /// <summary>
    /// Represent a response of server. It's a file colection container.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// The array of files
        /// </summary>
        public File[] files { get; set; }

        /// <summary>
        /// Create a new instance of Result with a new colection of files
        /// </summary>
        /// <param name="Files"></param>
        public Result(List<File> Files)
        {
            files = Files.ToArray();
        }
    }


    #endregion


}