using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteRecorderDemoGUI.PanoptoRemoteRecorderManagement;
using System.Threading;

namespace RemoteRecorderDemoGUI
{
    class RemoteRecorderWrapper
    {
        /// <summary>
        /// Starts a new recording session.
        /// </summary>
        /// <param name="recorder">Recorder Client to schedule the new session.</param>
        /// <param name="authInfo">Panopto user Authentication info.</param>
        /// <param name="sessionLength">The session duration in minutes.</param>
        /// <param name="sessionName">The new session name.</param>
        /// <param name="folderId">The id of the folder where the new session will be located.</param>
        /// <param name="recorderId">The id of the remote recorder that will be used to record the new session.</param>
        /// <returns>Returns the schedule result.</returns>
        public static ScheduledRecordingResult StartRecordingSession(RemoteRecorderManagementClient recorder, AuthenticationInfo authInfo, int sessionLength, string sessionName, Guid folderId, Guid recorderId)
        {
            // Creates the recording settings list
            List<RecorderSettings> recorderSettings = new List<RecorderSettings>();
            ScheduledRecordingResult sr = null;
            try
            {
                // Sets the remote recording id in the recording settings
                recorderSettings.Add(new RecorderSettings { RecorderId = recorderId });

                // Schedules a new recording session that will start at the current time.
                sr = recorder.ScheduleRecording(authInfo, sessionName, folderId, false, DateTime.Now, DateTime.Now.AddMinutes(sessionLength), recorderSettings.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return sr;
        }

        /// <summary>
        /// Stops the recording of a session that is currently recording.
        /// </summary>
        /// <param name="recorder">Recorder Client that is executing the session.</param>
        /// <param name="authInfo">Panopto user Authenticaion info.</param>
        /// <param name="sessionId">The session to be stopped.</param>
        /// <returns>Returns the schedule result.</returns>
        public static ScheduledRecordingResult StopSessionRecording(RemoteRecorderManagementClient recorder, AuthenticationInfo authInfo, Guid sessionId)
        {
            // Updates the recording setting the current time as the finish time. That will stop the current session recording.
            return recorder.UpdateRecordingTime(authInfo, sessionId, DateTime.Now, DateTime.Now);
        }

        /// <summary>
        /// Get from server a full list of folders the given user has access to
        /// </summary>
        /// <param name="userID">User name</param>
        /// <param name="userKey">User password</param>
        /// <returns>Full list of folders to ID dictionary the user has access to</returns>
        public static Dictionary<string, Guid> GetFolders(string userID, string userKey)
        {
            // Variable used to get data
            PanoptoSessionManagement.SessionManagementClient sessionMgr = new PanoptoSessionManagement.SessionManagementClient();
            PanoptoSessionManagement.AuthenticationInfo sessionAuthInfo = new PanoptoSessionManagement.AuthenticationInfo()
                                                                          {
                                                                              UserKey = userID,
                                                                              Password = userKey
                                                                          };
            PanoptoSessionManagement.Pagination sessionPagination = new PanoptoSessionManagement.Pagination { MaxNumberResults = 10, PageNumber = 0 };

            // Get data once
            PanoptoSessionManagement.ListFoldersResponse response = sessionMgr.GetFoldersList(sessionAuthInfo,
                                                                                              new PanoptoSessionManagement.ListFoldersRequest { Pagination = sessionPagination },
                                                                                              null);

            Dictionary<string, Guid> folderInfo = new Dictionary<string, Guid>();
            System.Collections.ArrayList duplicates = new System.Collections.ArrayList();
            foreach (PanoptoSessionManagement.Folder folder in response.Results)
            {
                // Check for duplicates before storing
                if (duplicates.Contains(folder.Name))
                {
                    folderInfo.Add(folder.Name + " (" + folder.Id + ")", folder.Id);
                }
                else if (folderInfo.ContainsKey(folder.Name))
                {
                    string tempName = folder.Name;
                    Guid tempID = folderInfo[folder.Name];
                    folderInfo.Remove(folder.Name);

                    folderInfo.Add(tempName + " (" + tempID + ")", tempID);
                    folderInfo.Add(folder.Name + " (" + folder.Id + ")", folder.Id);
                    duplicates.Add(tempName);
                }
                else
                {
                    folderInfo.Add(folder.Name, folder.Id);
                }
            }

            // Get more data while there are more to get
            int totalResults = response.TotalNumberResults;
            int currentResults = 10;

            while (currentResults < totalResults)
            {
                sessionPagination.PageNumber += 1;
                response = sessionMgr.GetFoldersList(sessionAuthInfo,
                                                     new PanoptoSessionManagement.ListFoldersRequest { Pagination = sessionPagination },
                                                     null);
                foreach (PanoptoSessionManagement.Folder folder in response.Results)
                {
                    // Check duplicates
                    if (duplicates.Contains(folder.Name))
                    {
                        folderInfo.Add(folder.Name + " (" + folder.Id + ")", folder.Id);
                    }
                    else if (folderInfo.ContainsKey(folder.Name))
                    {
                        string tempName = folder.Name;
                        Guid tempID = folderInfo[folder.Name];
                        folderInfo.Remove(folder.Name);

                        folderInfo.Add(tempName + " (" + tempID + ")", tempID);
                        folderInfo.Add(folder.Name + " (" + folder.Id + ")", folder.Id);
                        duplicates.Add(tempName);
                    }
                    else
                    {
                        folderInfo.Add(folder.Name, folder.Id);
                    }
                }
                currentResults += 10;
            }

            return folderInfo;
        }

        /// <summary>
        /// Get full list of remote recorders the user has access to
        /// </summary>
        /// <param name="userID">User name</param>
        /// <param name="userKey">User password</param>
        /// <returns>Full list of remote recorders to their ID in a Dictionary</returns>
        public static Dictionary<string, Guid> GetRemoteRecorders(string userID, string userKey)
        {
            // Variable used to get data
            PanoptoRemoteRecorderManagement.RemoteRecorderManagementClient recorderMgr = new RemoteRecorderManagementClient();
            PanoptoRemoteRecorderManagement.AuthenticationInfo recorderAuthInfo = new PanoptoRemoteRecorderManagement.AuthenticationInfo()
            {
                UserKey = userID,
                Password = userKey
            };
            PanoptoRemoteRecorderManagement.Pagination recorderPagination = new PanoptoRemoteRecorderManagement.Pagination { MaxNumberResults = 10, PageNumber = 0 };

            // Get data once
            PanoptoRemoteRecorderManagement.ListRecordersResponse response = recorderMgr.ListRecorders(recorderAuthInfo,
                                                                                                       recorderPagination,
                                                                                                       PanoptoRemoteRecorderManagement.RecorderSortField.Name);

            Dictionary<string, Guid> remoteRecorderInfo = new Dictionary<string, Guid>();
            System.Collections.ArrayList duplicates = new System.Collections.ArrayList();
            foreach (RemoteRecorder rr in response.PagedResults)
            {
                // Check for duplicates before storing
                if (duplicates.Contains(rr.Name))
                {
                    remoteRecorderInfo.Add(rr.Name + " (" + rr.Id + ")", rr.Id);
                }
                else if (remoteRecorderInfo.ContainsKey(rr.Name))
                {
                    string tempName = rr.Name;
                    Guid tempID = remoteRecorderInfo[rr.Name];
                    remoteRecorderInfo.Remove(rr.Name);

                    remoteRecorderInfo.Add(tempName + " (" + tempID + ")", tempID);
                    remoteRecorderInfo.Add(rr.Name + " (" + rr.Id + ")", rr.Id);
                    duplicates.Add(tempName);
                }
                else
                {
                    remoteRecorderInfo.Add(rr.Name, rr.Id);
                }
            }

            // While there are more data remaining, get data
            int totalResults = response.TotalResultCount;
            int currentResults = 10;

            while (currentResults < totalResults)
            {
                recorderPagination.PageNumber += 1;
                response = recorderMgr.ListRecorders(recorderAuthInfo,
                                                     recorderPagination,
                                                     PanoptoRemoteRecorderManagement.RecorderSortField.Name);
                foreach (RemoteRecorder rr in response.PagedResults)
                {
                    // Check for duplicates
                    if (duplicates.Contains(rr.Name))
                    {
                        remoteRecorderInfo.Add(rr.Name + " (" + rr.Id + ")", rr.Id);
                    }
                    else if (remoteRecorderInfo.ContainsKey(rr.Name))
                    {
                        string tempName = rr.Name;
                        Guid tempID = remoteRecorderInfo[rr.Name];
                        remoteRecorderInfo.Remove(rr.Name);

                        remoteRecorderInfo.Add(tempName + " (" + tempID + ")", tempID);
                        remoteRecorderInfo.Add(rr.Name + " (" + rr.Id + ")", rr.Id);
                        duplicates.Add(tempName);
                    }
                    else
                    {
                        remoteRecorderInfo.Add(rr.Name, rr.Id);
                    }
                }
                currentResults += 10;
            }

            return remoteRecorderInfo;
        }
    }
}
