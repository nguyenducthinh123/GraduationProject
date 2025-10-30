using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System
{
    partial class Document
    {
        public string UserName { get => GetString(nameof(UserName)); set => Push(nameof(UserName), value); }
        public string Password { get => GetString(nameof(Password)); set => Push(nameof(Password), value); }
        public string Role { get => GetString(nameof(Role)); set => Push(nameof(Role), value); }
        public string Token { get { return GetString("token"); } set => Push("token", value); }
        public string Topic { get { return GetString("topic"); } set => Push("topic", value); }
        public long MessageID { get { return GetValue<long>("mid"); } set => Push("mid", value); }
        public string ClientID { get { return GetString("cid"); } set => Push("cid", value); }
        public bool Retain { get => GetValue<bool>("retain"); set => Push("retain", value); }
        public byte QoS { get => GetValue<byte>("qos"); set => Push("qos", value); }
        public long Timeout { get { return GetValue<long>(nameof(Timeout)); } set => Push(nameof(Timeout), value); }
        public string Avatar { get => GetString(nameof(Avatar)); set => Push(nameof(Avatar), value); }
        public string Email { get => GetString(nameof(Email)); set => Push(nameof(Email), value); }
        public string Device { get => GetString(nameof(Device)); set => Push(nameof(Device), value); }
        public string Model { get => GetString("model"); set => Push("model", value); }
        public string TrangThai { get => GetString(nameof(TrangThai)); set => Push(nameof(TrangThai), value); }
        public string Version { get => GetString("version"); set => Push("version", value); }
        public string GioiTinh { get => GetString(nameof(GioiTinh)); set => Push(nameof(GioiTinh), value); }
        public string SoDT { get => GetString(nameof(SoDT)); set => Push(nameof(SoDT), value); }
        public string ParentID { get => GetString(nameof(ParentID)); set => Push(nameof(ParentID), value); }

        public DateTime? LastAccess
        {
            get => GetDateTime(nameof(LastAccess));
            set => Push(nameof(LastAccess), value);
        }
        public Document Status
        {
            get => GetDocument(nameof(Status));
            set => Push(nameof(Status), value);
        }
    }
}

namespace System
{
    partial class Document
    {
        public DocumentList Items { get => GetDocumentList("items"); set => Push("items", value); }
        public List<string> Fields { get => GetArray<string>("fields"); set => Push("fields", value); }
        public string Link { get => GetString(nameof(Link)); set => Push(nameof(Link), value); }
        public int Vote { get => GetValue<int>(nameof(Vote)); set => Push(nameof(Vote), value); }
    }
}

namespace System
{
    using BsonData;
    partial class Document
    {
        public string ProfileId { get => GetString("pid"); set => Push("pid", value); }
        public int SoLuong { get => GetValue<int>(nameof(SoLuong)); set => Push(nameof(SoLuong), value); }
        public DateTime? Ngay { get => GetDateTime(nameof(Ngay)); set => Push(nameof(Ngay), value); }
        public DateTime? Gio { get => GetDateTime(nameof(Gio)); set => Push(nameof(Gio), value); }
        public DateTime? BD { get => GetDateTime(nameof(BD)); set => Push(nameof(BD), value); }
        public DateTime? KT { get => GetDateTime(nameof(KT)); set => Push(nameof(KT), value); }
    }

    partial class Document
    {
        static public Document Success(object value)
        {
            return new Document { Value = value };
        }
        static public Document Success(string url, object value)
        {
            return new Document { Url = url, Value = value };
        }
        static public Document Error(int code, string message)
        {
            return new Document {
                Code = code,
                Message = message,
            };
        }
        static public Document Error(string message) => Error(1, message);


        public DocumentList ToList()
        {
            var lst = new DocumentList();
            foreach (var p in this)
            {
                var e = JObject.ToDocument(p.Value);
                e.ObjectId = p.Key;
                lst.Add(e);
            }
            return lst;
        }
        public Document InnerJoin(params Collection[] tables)
        {
            var doc = Clone();
            var id = ObjectId;
            foreach (var t in tables)
            {
                var e = t.Find(id);
                if (e != null)
                {
                    doc.Copy(e);
                }
            }
            return doc;
        }
        public Document Load(params Collection[] tables)
        {
            var doc = Clone();
            var id = ObjectId;
            foreach (var t in tables)
            {
                var e = t.Find(id) ?? new Document();
                doc.Push(t.Name, e);
            }
            return doc;
        }
    }
}

namespace System
{
    partial class Document
    {

    }
}