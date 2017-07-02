using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;

namespace GTPTracker.VM
{
    public class CommentDetailsVM
    {
        repoComments repComments = new repoComments();
        repoCommentFiles repFiles = new repoCommentFiles();

        public Comments comment;
        public IEnumerable<CommentsFiles> lFiles;

        public CommentDetailsVM(int idTask)
        {
            comment = repComments.get(idTask);
            lFiles = repFiles.getList(idTask);
        }
    }
}