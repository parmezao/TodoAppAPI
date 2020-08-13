using System;
using Flunt.Notifications;
using Flunt.Validations;
using Todo.Domain.Commands.Contracts;

namespace Todo.Domain.Commands
{
    public class DeleteTodoCommand : Notifiable, ICommand
    {
        public DeleteTodoCommand() { }

        public DeleteTodoCommand(Guid id, string user)
        {
            Id = id;
            User = user;
        }

        public Guid Id { get; set; }
        public string User { get; set; }

        public void Validate()
        {
            AddNotifications(
                new Contract()
                    .Requires()
                    .HasMinLen(User, 6, "User", "Usuário inválido!")
                    .IsFalse(Id == Guid.Empty, "Id", "Tarefa não informada!")
            );
        }
    }
}