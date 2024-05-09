using NPOI.SS.UserModel;

namespace SP1.Cache
{
    public class WorkbookCache
    {
        //lancana lista koja predstavlja thread safe kes sa maksimalnim dozvoljenim brojem elemenata
        //kes implementira LRU algoritam
        //sortiran je od najskorije koriscenog (prvi) do najranije koriscenog (zadnji)
        //elementi se uvek dodaju na pocetak i oduzimaju sa kraja
        private readonly LinkedList<IWorkbook> cache = new();
        private readonly int maxSize;
        public readonly ReaderWriterLockSlim rwLock = new();

        public WorkbookCache(int size)
        {
            maxSize = size;
        }

        //oznacava da se koristi workbook zadat imenom sheet-a
        //ako je workbook u kesu pomera se na pocetak (postaje najskorije koriscen)
        //ako nije dodaje se na pocetak i po potrebi se brise zadnji (najranije korisceni)
        public void AddOrUse(IWorkbook value)
        {
            rwLock.EnterWriteLock();
            if (cache.Contains(value))
            {
                cache.Remove(value);
            }
            else if (cache.Count >= maxSize)
            {
                cache.RemoveLast();
            }

            cache.AddFirst(value);
            rwLock.ExitWriteLock();
        }

        //pokusava da preuzme workbook sa zadatim imenom sheet-a
        //ime sheet-a predstavlja ime csv fajla, znaci da je jedinstveno u kesu
        //ako workbook nije u kesu vraca null
        public IWorkbook? TryGet(string name)
        {
            rwLock.EnterReadLock();
            if (cache.Count == 0)
            {
                rwLock.ExitReadLock();
                return null;
            }

            foreach (IWorkbook wb in cache)
            {
                if (wb.GetSheetName(0) == name)
                {
                    rwLock.ExitReadLock();
                    return wb;
                }
            }

            rwLock.ExitReadLock();
            return null;
        }
    }
}
