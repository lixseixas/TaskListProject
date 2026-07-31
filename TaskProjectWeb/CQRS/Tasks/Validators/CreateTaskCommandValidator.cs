using FluentValidation;
using System;
using TaskProject.CQRS.Tasks.Commands;
using TaskProject.Models;

namespace TaskProject.CQRS.Tasks.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Task.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Task.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Task.Date)
            .NotEmpty().WithMessage("Date is required");

        RuleFor(x => x.Task.InitialHour)
            .NotEmpty().WithMessage("Initial hour is required");

        RuleFor(x => x.Task.FinalHour)
            .NotEmpty().WithMessage("Final hour is required")
            .Must((command, finalHour) => finalHour != command.Task.InitialHour)
            .WithMessage("Final hour must be different from initial hour");

        RuleFor(x => x.Task.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5");

        RuleFor(x => x.Task)
            .Must(task => ValidateTaskDates(task))
            .WithMessage("Invalid task dates or hours")
            .Must(task => ValidateTaskDuration(task))
            .WithMessage("Task duration cannot exceed 5 hours");
    }

    private bool ValidateTaskDates(TaskModel task)
    {
        var dataInicioTask = task.Date.AddHours(task.InitialHour.Hour).AddMinutes(task.InitialHour.Minute);
        var dataAgora = DateTime.Now;
        
        if (dataInicioTask < dataAgora)
            return false;

        var dataFimTask = task.Date.AddHours(task.FinalHour.Hour).AddMinutes(task.FinalHour.Minute);
        
        if (dataFimTask < dataAgora)
            return false;

        if (dataFimTask < dataInicioTask)
            return false;

        return true;
    }

    private bool ValidateTaskDuration(TaskModel task)
    {
        var hoursTask = task.FinalHour.Subtract(task.InitialHour);
        var minutosTotais = hoursTask.TotalMinutes;
        return minutosTotais <= 300;
    }
}
