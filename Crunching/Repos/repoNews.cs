using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GTPTracker.repos
{
    public class repoNews
    {
        public IEnumerable<News> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.News.ToList();
            }
        }

        public News get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.News.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(News item)
        {
            using (var db = new GTPTrackerEntities())
            {
                item.cDate = DateTime.UtcNow;
                db.News.AddObject(item);
                db.SaveChanges();
                repoItems repItems = new repoItems();
                Items news = new Items();
                news.cDate = DateTime.UtcNow;
                news.idType = 3;
                news.link = item.id;
                news.title = item.title;
                news.isInternal = false;
                repItems.Create(news);
            }
        }

        public void addUser(int idNews, int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                NewsWatchList element = new NewsWatchList();
                element.idNews = idNews;
                element.idUser = idUser;
                db.NewsWatchList.AddObject(element);
                db.SaveChanges();
            }
        }

        public IEnumerable<NewsWatchList> getMyNews(int idUser)
        {
            using (var db = new GTPTrackerEntities())
                return db.NewsWatchList.Where(p => p.idUser == idUser).ToList();
        }
    }
}