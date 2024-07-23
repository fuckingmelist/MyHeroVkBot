using MyHeroVkBot.DbConnector;
using MyHeroVkBot.Models;
using MyHeroVkBot.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace MyHeroVkBot.Services
{
    public class MainService
    {
        private static string _accessToken = "vk1.a.993a-LO02EGtXdMLe17AkU2DK1gEyZ_qOEt49QU_Kd3Yhbvc5VzaA6vWCfPTSaBLcHwVty2_6hYsrLb0MwSQe2BlVS7KNNpOpVybHYOeN5ODrYdDYg1wuE8I9uf1CUWDILICXnSGNdzURfB0J2EKpZ5fqr42LDt6Py0k_h_UXzJwp3zKpVRCUjMytFK7zlnzWzmQRWq8sOl6_kROxQsMww";

        public BotMessage ProcessCommandStart(Message message, TransmittedData transmittedData)
        {
            transmittedData.State = States.WaitingAction;

            using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
            {
                Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                user.State = States.WaitingAction;
                db.Users.Update(user);
                db.SaveChanges();
            }

            return new BotMessage(
                    "Выберите материал для отправки:\n\n(Вы можете также заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData)
                );

            throw new Exception("Неизвестная ошибка в ProcessCommandStart");
        }

        public BotMessage ProcessWaiting(Message message, TransmittedData data)
        {
            using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
            {
                UserHero uh = null;

                try
                {
                    uh = db.UserHeroes.Where(x => x.UserId == message.FromId).Last();
                }
                catch
                {
                    uh = null;
                }

                if (uh == null)
                {
                    if (message.Text == "Отправить заново")
                    {
                        data.State = States.WaitingAction;
                        data.Storage.Clear((long)message.FromId);

                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingAction;
                        db.Users.Update(user);
                        db.SaveChanges();

                        return new BotMessage("Выберите действие:", BuildActionsKeyBoard(data));
                    }
                    else
                    {
                        KeyboardBuilder kb = new KeyboardBuilder();
                        kb.AddButton("Отправить заново", "", KeyboardButtonColor.Default);
                        kb.SetOneTime();

                        return new BotMessage("Если вы хотите отправить героя заново, нажмите на кнопку", kb.Build());
                    }
                }
                else
                {
                    Hero hero = db.Heroes.Where(x => x.Id == uh.HeroId).FirstOrDefault();

                    if (hero.Accepted == false)
                    {
                        return new BotMessage("Заявка на проверке, пожалуйста подождите.", null);
                    }
                    else
                    {
                        if (message.Text == "Отправить нового героя")
                        {
                            data.State = States.WaitingAction;
                            data.Storage.Clear((long)message.FromId);

                            Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                            user.State = States.WaitingAction;
                            db.Users.Update(user);
                            db.SaveChanges();

                            return new BotMessage("Выберите действие:", BuildActionsKeyBoard(data));
                        }
                        else
                        {
                            KeyboardBuilder kb = new KeyboardBuilder();
                            kb.AddButton("Отправить нового героя", "", KeyboardButtonColor.Default);
                            kb.SetOneTime();

                            return new BotMessage("Если вы хотите отправить нового героя, нажмите на кнопку", kb.Build());
                        }
                    }
                }
            }


            throw new NotImplementedException();
        }

        public BotMessage ProcessAdminActions(Message message, TransmittedData data)
        {
            KeyboardBuilder keyboard = new KeyboardBuilder();

            switch (message.Text)
            {
                case "Принять заявки":

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Hero hero = db.Heroes.Where(x => x.Accepted == false && x.CheckingUserId == null).FirstOrDefault();

                        if (hero != null)
                        {
                            data.State = States.WaitingAcceptance;
                            data.Storage.AddOrUpdate("currentHeroId", hero.Id, (long)message.FromId);
                            hero.CheckingUserId = (long)message.FromId;
                            db.Heroes.Update(hero);
                            db.SaveChanges();

                            Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                            user.State = States.WaitingAcceptance;
                            db.Users.Update(user);
                            db.SaveChanges();

                            keyboard.AddButton("Принять", "", KeyboardButtonColor.Positive);
                            keyboard.AddButton("Отклонить", "", KeyboardButtonColor.Negative);
                            keyboard.AddLine();
                            keyboard.AddButton("Назад", "", KeyboardButtonColor.Default);
                            keyboard.SetOneTime();

                            if (hero.VideoId != null)
                                return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId, (long)hero.VideoId } };
                            else
                                return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId } };

                        }
                        else
                        {
                            keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                            keyboard.AddLine();
                            keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                            keyboard.SetOneTime();

                            return new BotMessage("Заявок пока нет...\n\nВыберите действие:", keyboard.Build());
                        }
                    }
                    break;
                case "Принятые заявки":

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Hero hero = db.Heroes.Where(x => x.Accepted == true).FirstOrDefault();

                        if (hero != null)
                        {
                            data.State = States.WaitingAcceptingApplications;
                            data.Storage.AddOrUpdate("page", 2, (long)message.FromId);

                            Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                            user.State = States.WaitingAcceptingApplications;
                            db.Users.Update(user);
                            db.SaveChanges();

                            keyboard.AddButton("Дальше", "", KeyboardButtonColor.Positive);
                            keyboard.AddLine();
                            keyboard.AddButton("В главное меню", "", KeyboardButtonColor.Default);

                            if (hero.VideoId != null)
                                return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId, (long)hero.VideoId } };
                            else
                                return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId } };
                        }
                        else
                        {
                            keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                            keyboard.AddLine();
                            keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                            keyboard.SetOneTime();

                            return new BotMessage("Принятых заявок пока нет...\n\nВыберите действие:", keyboard.Build());
                        }
                    }
                    break;
                default:
                    keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                    keyboard.AddLine();
                    keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                    keyboard.SetOneTime();

                    return new BotMessage(
                  "Неправильная команда, нажмите на кнопку", keyboard.Build()
              );
                    break;
            }

            throw new NotImplementedException();
        }

        public BotMessage ProcessReason(Message message, TransmittedData data)
        {
            if (message.Text != "Назад")
            {
                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Hero hero = db.Heroes.Where(x => x.Id == int.Parse(data.Storage.Get("currentHeroId").ToString())).FirstOrDefault();
                    db.Heroes.Remove(hero);

                    UserHero userHero = db.UserHeroes.Where(x => x.HeroId == hero.Id).FirstOrDefault();

                    db.UserHeroes.Remove(userHero);
                    db.SaveChanges();

                    using (VkApi api = new VkApi())
                    {
                        api.Authorize(new ApiAuthParams
                        {
                            AccessToken = _accessToken
                        });

                        Random rnd = new Random();

                        KeyboardBuilder keyboardCancel = new KeyboardBuilder();

                        keyboardCancel.AddButton("Отправить заново", "", KeyboardButtonColor.Default);
                        keyboardCancel.SetOneTime();

                        api.Messages.SendAsync(new MessagesSendParams
                        {
                            RandomId = rnd.Next(0, 1000000000),
                            UserId = userHero.UserId,
                            Message = $"Ваша заявка на героя отклонена. Причина: {message.Text}\nВы можете попробовать отправить героя заново, для этого нажмите на кнопку",
                            Keyboard = keyboardCancel.Build()
                        });
                    }
                    Thread.Sleep(100);
                }

                KeyboardBuilder keyboard = new KeyboardBuilder();

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Hero hero = db.Heroes.Where(x => x.Accepted == false && x.CheckingUserId == null).FirstOrDefault();

                    if (hero != null)
                    {
                        data.State = States.WaitingAcceptance;
                        data.Storage.AddOrUpdate("currentHeroId", hero.Id, (long)message.FromId);
                        hero.CheckingUserId = (long)message.FromId;
                        db.Heroes.Update(hero);
                        db.SaveChanges();

                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingAcceptance;
                        db.Users.Update(user);
                        db.SaveChanges();

                        keyboard.AddButton("Принять", "", KeyboardButtonColor.Positive);
                        keyboard.AddButton("Отклонить", "", KeyboardButtonColor.Negative);
                        keyboard.AddLine();
                        keyboard.AddButton("Назад", "", KeyboardButtonColor.Default);
                        keyboard.SetOneTime();

                        if (hero.VideoId != null)
                            return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId, (long)hero.VideoId } };
                        else
                            return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId } };
                    }
                    else
                    {
                        data.State = States.WaitingAdminActions;

                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingAdminActions;
                        db.Users.Update(user);
                        db.SaveChanges();

                        keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                        keyboard.AddLine();
                        keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                        keyboard.SetOneTime();

                        return new BotMessage("Заявок больше нет...\n\nВыберите действие:", keyboard.Build());
                    }
                }
            }
            else
            {
                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Hero hero = db.Heroes.Where(x => x.Id == int.Parse(data.Storage.Get("currentHeroId").ToString())).FirstOrDefault();

                    data.State = States.WaitingAcceptance;

                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAcceptance;
                    db.Users.Update(user);
                    db.SaveChanges();

                    KeyboardBuilder keyboard = new KeyboardBuilder();
                    keyboard.AddButton("Принять", "", KeyboardButtonColor.Positive);
                    keyboard.AddButton("Отклонить", "", KeyboardButtonColor.Negative);
                    keyboard.AddLine();
                    keyboard.AddButton("Назад", "", KeyboardButtonColor.Default);
                    keyboard.SetOneTime();

                    if (hero.VideoId != null)
                        return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId, (long)hero.VideoId } };
                    else
                        return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId } };

                }
            }

            throw new NotImplementedException();
        }

        public BotMessage ProcessAcceptingApplications(Message message, TransmittedData data)
        {
            KeyboardBuilder keyboard = new KeyboardBuilder();
            if (message.Text != "В главное меню")
            {
                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    int page = int.Parse(data.Storage.Get("page").ToString());

                    Hero hero = db.Heroes.Where(x => x.Accepted == true).Skip((page - 1) * 1).Take(1).FirstOrDefault();

                    if (hero != null)
                    {
                        data.Storage.AddOrUpdate("page", page + 1, (long)message.FromId);


                        keyboard.AddButton("Дальше", "", KeyboardButtonColor.Positive);
                        keyboard.AddLine();
                        keyboard.AddButton("В главное меню", "", KeyboardButtonColor.Default);

                        if (hero.VideoId != null)
                            return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId, (long)hero.VideoId } };
                        else
                            return new BotMessage(hero.Text, keyboard.Build()) { Array = new[] { (long)hero.PhotoId, (long)hero.AudioId } };
                    }
                    else
                    {
                        data.State = States.WaitingAdminActions;

                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingAdminActions;
                        db.Users.Update(user);
                        db.SaveChanges();

                        keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                        keyboard.AddLine();
                        keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                        keyboard.SetOneTime();

                        return new BotMessage("Принятых заявок пока нет...\n\nВыберите действие:", keyboard.Build());
                    }
                }
            }
            else
            {
                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    data.State = States.WaitingAdminActions;

                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAdminActions;
                    db.Users.Update(user);
                    db.SaveChanges();

                    keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                    keyboard.AddLine();
                    keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                    keyboard.SetOneTime();

                    return new BotMessage("Выберите действие:", keyboard.Build());
                }
            }

            throw new NotImplementedException();
        }

        public BotMessage ProcessAdminStart(Message message, TransmittedData data)
        {
            data.State = States.WaitingAdminActions;

            using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
            {
                Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                user.State = States.WaitingAdminActions;
                db.Users.Update(user);
                db.SaveChanges();
            }

            KeyboardBuilder keyboard = new KeyboardBuilder();

            keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
            keyboard.AddLine();
            keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
            keyboard.SetOneTime();

            return new BotMessage(
                 "Выберите действие:", keyboard.Build()
             );

            throw new NotImplementedException();
        }

        public BotMessage ProcessAcceptance(Message message, TransmittedData data)
        {
            KeyboardBuilder keyboard = new KeyboardBuilder();

            switch (message.Text)
            {
                case "Принять":

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Hero hero = db.Heroes.Where(x => x.Id == int.Parse(data.Storage.Get("currentHeroId").ToString())).FirstOrDefault();
                        hero.Accepted = true;
                        db.Heroes.Update(hero);
                        db.SaveChanges();

                        UserHero userHero = db.UserHeroes.Where(x => x.HeroId == hero.Id).FirstOrDefault();

                        using (VkApi api = new VkApi())
                        {
                            api.Authorize(new ApiAuthParams
                            {
                                AccessToken = _accessToken
                            });

                            Random rnd = new Random();

                            KeyboardBuilder keyboardAccepted = new KeyboardBuilder();

                            keyboardAccepted.AddButton("Отправить нового героя", "", KeyboardButtonColor.Default);
                            keyboardAccepted.SetOneTime();

                            api.Messages.SendAsync(new MessagesSendParams
                            {
                                RandomId = rnd.Next(0, 1000000000),
                                UserId = userHero.UserId,
                                Message = $"Ваша заявка на героя принята!!!\nВы можете отправить ещё одного героя, для этого нажмите на кнопку",
                                Keyboard = keyboardAccepted.Build()
                            });
                        }
                        Thread.Sleep(100);

                        Hero newHero = db.Heroes.Where(x => x.Accepted == false && x.CheckingUserId == null).FirstOrDefault();

                        if (newHero != null)
                        {
                            data.Storage.AddOrUpdate("currentHeroId", newHero.Id, (long)message.FromId);

                            newHero.CheckingUserId = (long)message.FromId;
                            db.Heroes.Update(newHero);
                            db.SaveChanges();

                            keyboard.AddButton("Принять", "", KeyboardButtonColor.Positive);
                            keyboard.AddButton("Отклонить", "", KeyboardButtonColor.Negative);
                            keyboard.AddLine();
                            keyboard.AddButton("Назад", "", KeyboardButtonColor.Default);
                            keyboard.SetOneTime();

                            if (newHero.VideoId != null)
                                return new BotMessage(newHero.Text, keyboard.Build()) { Array = new[] { (long)newHero.PhotoId, (long)newHero.AudioId, (long)newHero.VideoId } };
                            else
                                return new BotMessage(newHero.Text, keyboard.Build()) { Array = new[] { (long)newHero.PhotoId, (long)newHero.AudioId } };
                        }
                        else
                        {
                            data.State = States.WaitingAdminActions;

                            Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                            user.State = States.WaitingAdminActions;
                            db.Users.Update(user);
                            db.SaveChanges();

                            keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                            keyboard.AddLine();
                            keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                            keyboard.SetOneTime();

                            return new BotMessage("Заявок больше нет...\n\nВыберите действие:", keyboard.Build());
                        }
                    }

                    break;
                case "Отклонить":
                    data.State = States.WaitingReason;

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingReason;
                        db.Users.Update(user);
                        db.SaveChanges();
                    }

                    keyboard.AddButton("Назад", "", KeyboardButtonColor.Default);
                    keyboard.SetOneTime();
                    return new BotMessage("Напишите причину отклонения", keyboard.Build());
                    break;

                case "Назад":
                    data.State = States.WaitingAdminActions;

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Hero hero = db.Heroes.Where(x => x.Id == int.Parse(data.Storage.Get("currentHeroId").ToString())).FirstOrDefault();
                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingAdminActions;
                        db.Users.Update(user);
                        hero.CheckingUserId = null;
                        db.Update(hero);
                        db.SaveChanges();
                    }

                    keyboard.AddButton("Принять заявки", "", KeyboardButtonColor.Positive);
                    keyboard.AddLine();
                    keyboard.AddButton("Принятые заявки", "", KeyboardButtonColor.Default);
                    keyboard.SetOneTime();

                    return new BotMessage(
                         "Выберите действие:", keyboard.Build()
                     );
                    break;
                default:
                    keyboard.AddButton("Принять", "", KeyboardButtonColor.Positive);
                    keyboard.AddButton("Отклонить", "", KeyboardButtonColor.Negative);
                    keyboard.AddLine();
                    keyboard.AddButton("Назад", "", KeyboardButtonColor.Default);
                    keyboard.SetOneTime();

                    return new BotMessage(
                      "Неправильная команда, нажмите на кнопку", keyboard.Build()
                  );
                    break;
            }

            throw new Exception("Неизвестная ошибка в ProcessAcceptance");
        }

        public MessageKeyboard BuildKeyboardExit()
        {
            KeyboardBuilder keyboard = new KeyboardBuilder();
            keyboard.AddButton("Назад", "", KeyboardButtonColor.Default);
            keyboard.SetOneTime();

            return keyboard.Build();
        }

        public BotMessage ProcessAction(Message message, TransmittedData transmittedData)
        {
            KeyboardBuilder keyboard = new KeyboardBuilder();

            switch (message.Text)
            {
                case "Фото":
                    transmittedData.State = States.WaitingPhoto;

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingPhoto;
                        db.Users.Update(user);
                        db.SaveChanges();
                    }
                    return new BotMessage(
               "Критерии к фотоматериалу:\n- минимум 1 фотография человека\n- фотографии медалей, сертификатов, орденов и тд. (при наличии)\n- чёткие, не размытые изображения" +
               "\n(отправьте одним сообщением, не как файл а как фотографии)", BuildKeyboardExit());
                    break;
                case "Видео":
                    transmittedData.State = States.WaitingVideo;

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingVideo;
                        db.Users.Update(user);
                        db.SaveChanges();
                    }
                    return new BotMessage(
                  "Критерии к видеоматериалу:\n- видео в максимально разрешённом качестве\n- отсутствие фонового шума\n- не деформированное изображение", BuildKeyboardExit());
                    break;
                case "Аудио":
                    transmittedData.State = States.WaitingAudio;

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingAudio;
                        db.Users.Update(user);
                        db.SaveChanges();
                    }
                    return new BotMessage(
                "Критерии к аудиоматериалу:\n- хороший звук\n- целостный аудиофайл в единичном экземляре\n- отсутствие фоновового шума\n- чёткая речь, без запинок, искажения слов и необоснованных пауз\n- аудиофайл полностью соответствует тексту", BuildKeyboardExit());
                    break;
                case "Текст":
                    transmittedData.State = States.WaitingText;

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingText;
                        db.Users.Update(user);
                        db.SaveChanges();
                    }
                    return new BotMessage(
               "Критерии к текстовому материалу:\n- текст в электронном формате, не написан от руки\n- текст должен быть не менее 80 слов\n- текст полностью соответствует аудиофайлу", BuildKeyboardExit());
                    break;
                case "Посмотреть загруженный материал":
                    List<long> array = new List<long>();
                    string text = "";


                    if (transmittedData.Storage.ContainsKey("Audio"))
                        array.Add((long)transmittedData.Storage.Get("Audio"));

                    if (transmittedData.Storage.ContainsKey("Photo"))
                        array.Add((long)transmittedData.Storage.Get("Photo"));

                    if (transmittedData.Storage.ContainsKey("Video"))
                        array.Add((long)transmittedData.Storage.Get("Video"));

                    if (transmittedData.Storage.ContainsKey("Text"))
                        text = (string)transmittedData.Storage.Get("Text");

                    if (text != "" || array.Count != 0)
                    {
                        return new BotMessage(
               text, BuildActionsKeyBoard(transmittedData))
                        { Array = array };
                    }
                    else
                    {
                        return new BotMessage(
                   "я", BuildActionsKeyBoard(transmittedData));
                    }

                    break;
                case "Отправить всё":
                    if (transmittedData.Storage != null)
                    {
                        if (transmittedData.Storage.ContainsKey("Audio"))
                        {
                            if (transmittedData.Storage.ContainsKey("Photo"))
                            {
                                if (transmittedData.Storage.ContainsKey("Text"))
                                {
                                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                                    {
                                        Hero hero = new Hero() { AudioId = (long)transmittedData.Storage.Get("Audio"), PhotoId = (long)transmittedData.Storage.Get("Photo"), Text = (string)transmittedData.Storage.Get("Text") };

                                        if (transmittedData.Storage.ContainsKey("Video"))
                                            hero.VideoId = (long)transmittedData.Storage.Get("Video");

                                        using (VkApi api = new VkApi())
                                        {
                                            api.Authorize(new ApiAuthParams
                                            {
                                                AccessToken = _accessToken
                                            });

                                            Random rnd = new Random();

                                            List<Models.User> admins = db.Users.Where(x => x.IsAdmin == true).ToList();

                                            if (admins != null)
                                            {
                                                if (admins.Count != 0)
                                                {
                                                    for (int i = 0; i < admins.Count; i++)
                                                    {
                                                        api.Messages.SendAsync(new MessagesSendParams
                                                        {
                                                            RandomId = rnd.Next(0, 1000000000),
                                                            UserId = admins[i].Id,
                                                            Message = "Пришла новая заявка на героя.",
                                                        });
                                                        Thread.Sleep(100);
                                                    }
                                                }
                                            }
                                        }

                                        transmittedData.State = States.Waiting;

                                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                                        user.State = States.Waiting;
                                        db.Users.Update(user);
                                        db.SaveChanges();

                                        return new BotMessage(
                      "Ваша заявка на публикацию героя отправлена. Дожидайтесь ответа.", null);
                                    }
                                }
                                else
                                    return new BotMessage(
                       "Вы не загрузили текст!", BuildActionsKeyBoard(transmittedData));
                            }
                            else
                                return new BotMessage(

               "Вы не загрузили фото!", BuildActionsKeyBoard(transmittedData));
                        }
                        else
                            return new BotMessage(
               "Вы не загрузили аудио!", BuildActionsKeyBoard(transmittedData));
                    }
                    else
                        return new BotMessage(
           "Вы ещё не загрузили никаких данных!", BuildActionsKeyBoard(transmittedData));

                    break;
                default:
                    return new BotMessage(
               "Для взамодействия нажмите на кнопку!", BuildActionsKeyBoard(transmittedData));
            }

            throw new Exception("Неизвестная ошибка в ProcessCommandStart");
        }

        public MessageKeyboard BuildActionsKeyBoard(TransmittedData transmittedData)
        {
            KeyboardBuilder keyboard = new KeyboardBuilder();

            if (transmittedData.Storage.ContainsKey("Video"))
                keyboard.AddButton("Видео", "", KeyboardButtonColor.Positive);
            else
                keyboard.AddButton("Видео", "", KeyboardButtonColor.Default);

            if (transmittedData.Storage.ContainsKey("Audio"))
                keyboard.AddButton("Аудио", "", KeyboardButtonColor.Positive);
            else
                keyboard.AddButton("Аудио", "", KeyboardButtonColor.Default);

            keyboard.AddLine();

            if (transmittedData.Storage.ContainsKey("Photo"))
                keyboard.AddButton("Фото", "", KeyboardButtonColor.Positive);
            else
                keyboard.AddButton("Фото", "", KeyboardButtonColor.Default);

            if (transmittedData.Storage.ContainsKey("Text"))
                keyboard.AddButton("Текст", "", KeyboardButtonColor.Positive);
            else
                keyboard.AddButton("Текст", "", KeyboardButtonColor.Default);

            keyboard.AddLine();
            keyboard.AddButton("Посмотреть загруженный материал", "", KeyboardButtonColor.Default);
            keyboard.AddLine();
            keyboard.AddButton("Отправить всё", "", KeyboardButtonColor.Primary);
            keyboard.SetOneTime();

            return keyboard.Build();
        }

        public BotMessage ProcessText(Message message, TransmittedData transmittedData)
        {
            if (message.Text == "Назад")
            {
                transmittedData.State = States.WaitingAction;

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }

                return new BotMessage("Выберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData));
            }
            else if (message.Text != "")
            {
                if (message.Text.Length > 500)
                {
                    transmittedData.State = States.WaitingAction;
                    transmittedData.Storage.AddOrUpdate("Text", message.Text, (long)message.FromId);

                    using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                    {
                        Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                        user.State = States.WaitingAction;
                        db.Users.Update(user);
                        db.SaveChanges();
                    }

                    return new BotMessage(
                 "Принято! \nВыберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData)
             );
                }
                else
                    return new BotMessage(
             "Текст должен быть не менее 80 слов", BuildKeyboardExit());
            }
            else
                return new BotMessage(
         "Вы пытаетесь отправить не текст", BuildKeyboardExit());

            throw new Exception("Неизвестная ошибка в ProcessText");
        }

        public BotMessage ProcessAudio(Message message, TransmittedData transmittedData)
        {
            if (message.Text == "Назад")
            {
                transmittedData.State = States.WaitingAction;

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }

                return new BotMessage("Выберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData));
            }
            else if (CheckRightType(message, "Audio"))
            {
                transmittedData.State = States.WaitingAction;
                transmittedData.Storage.AddOrUpdate("Audio", message.Id, (long)message.FromId);

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }

                return new BotMessage(
                 "Принято! \nВыберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData)
             );
            }
            else if (CheckRightType(message, "AudioMessage"))
            {
                transmittedData.State = States.WaitingAction;
                transmittedData.Storage.AddOrUpdate("Audio", message.Id, (long)message.FromId);

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }

                return new BotMessage(
              "Принято! \nВыберите материал для отправки:\n\n(Вы можете также заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData)
          );
            }
            else
                return new BotMessage(
          "Вы пытаетесь отправить не аудиофайл", BuildKeyboardExit()
      );

            throw new Exception("Неизвестная ошибка в ProcessAudio");
        }

        public BotMessage ProcessVideo(Message message, TransmittedData transmittedData)
        {
            if (message.Text == "Назад")
            {
                transmittedData.State = States.WaitingAction;

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }
                return new BotMessage("Выберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData));
            }
            else if (CheckRightType(message, "Video"))
            {
                transmittedData.State = States.WaitingAction;

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }

                transmittedData.Storage.AddOrUpdate("Video", message.Id, (long)message.FromId);

                return new BotMessage(
                 "Принято! \nВыберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData)
             );
            }
            else
                return new BotMessage(
          "Вы пытаетесь отправить не видео", BuildKeyboardExit()
      );

            throw new Exception("Неизвестная ошибка в ProcessVideo");
        }

        public BotMessage ProcessPhoto(Message message, TransmittedData transmittedData)
        {
            if (message.Text == "Назад")
            {
                transmittedData.State = States.WaitingAction;

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }

                return new BotMessage("Выберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData));
            }
            else if (CheckRightType(message, "Photo"))
            {
                transmittedData.State = States.WaitingAction;
                transmittedData.Storage.AddOrUpdate("Photo", message.Id, (long)message.FromId);

                using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
                {
                    Models.User user = db.Users.Where(x => x.Id == message.FromId).FirstOrDefault();
                    user.State = States.WaitingAction;
                    db.Users.Update(user);
                    db.SaveChanges();
                }

                return new BotMessage(
                 "Принято! \nВыберите материал для отправки:\n\n(Вы также можете заменить выбранный материал, нажав на соответсвующую кнопку и повторно отправив новый)", BuildActionsKeyBoard(transmittedData)
             );
            }
            else
                return new BotMessage(
          "Вы пытаетесь отправить не фото", BuildKeyboardExit()
      );

            throw new Exception("Неизвестная ошибка в ProcessPhoto");
        }

        private bool CheckRightType(Message msg, string parameter)
        {
            if (msg.Attachments.Count != 0)
            {
                if (msg.Attachments[0] != null)
                {
                    if (msg.Attachments[0].Type.Name == parameter)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }
    }
}
