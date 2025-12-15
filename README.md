# Connectly - a micro-social platform developed in .NET 

>[!warning]
> Still in development.

## Setup 

Clone the repo 

```
git clone https://github.com/stefanchp/Connectly.git && cd Connectly
```

Create the docker container and run the configuration. 

```
docker compose up -d
```

Setup an `.env` file:

```
cp .env.example .env
```

And then setup a `DB_PASSWORD` in `.env` and connect with a connection string in `appsettings.Development.json`.

To update the database run:

```
dotnet ef database update
```

## Database Diagram

```mermaid
erDiagram
    AspNetUsers {
        string Id PK
        string FullName
        string Bio
        string ProfileImageUrl
        bool IsPrivate
        string UserName
        string NormalizedUserName
        string Email
    }
    FollowRequests {
        int Id PK
        string FromUserId FK
        string ToUserId FK
        int Status
        datetime CreatedAt
        datetime RespondedAt
    }
    Groups {
        int Id PK
        string Name
        string Description
        string CreatedById FK
        datetime CreatedAt
    }
    GroupMembers {
        int Id PK
        int GroupId FK
        string UserId FK
        int Role
        int Status
        datetime CreatedAt
    }
    GroupMessages {
        int Id PK
        int GroupId FK
        string UserId FK
        string Content
        datetime CreatedAt
    }

    AspNetUsers ||--o{ FollowRequests : "from"
    AspNetUsers ||--o{ FollowRequests : "to"
    AspNetUsers ||--o{ Groups : "creates"
    AspNetUsers ||--o{ GroupMembers : "joins"
    AspNetUsers ||--o{ GroupMessages : "writes"
    Groups ||--o{ GroupMembers : "has members"
    Groups ||--o{ GroupMessages : "has messages"
```
